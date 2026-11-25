using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Bugtrack.Api.Enums;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

public class TicketService : ITicketService
{
    private readonly AppDbContext db;

    public TicketService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<TicketResponse> CreateAsync(
        Guid userId,
        Guid projectId,
        CreateTicketRequest data)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        await LockProjectAsync(projectId);

        var member = await RequireMemberAsync(userId, projectId);

        RequireReporter(member);

        await ValidateSoftwareAsync(projectId, data.SoftwareId);

        ValidatePriority(data.Priority);

        var number = await db.Tickets.CountAsync(
            item => item.ProjectId == projectId) + 1;

        var code = $"BUG-{number}";

        while (await db.Tickets.AnyAsync(item =>
            item.ProjectId == projectId &&
            item.Code == code))
        {
            number++;
            code = $"BUG-{number}";
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            CreatorId = userId,
            Code = code,
            Status = TicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        ApplyFields(ticket, data);

        db.Tickets.Add(ticket);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await ReadResponseAsync(projectId, ticket.Id);
    }

    public async Task<PagedTicketsResponse> GetByProjectAsync(
        Guid userId,
        Guid projectId,
        TicketQueryRequest data)
    {
        await RequireMemberAsync(userId, projectId);

        Require(
            data.Page >= 1 &&
            data.PageSize >= 1 &&
            data.PageSize <= 100,
            400,
            "Некорректная пагинация.");

        Require(
            !data.Status.HasValue ||
            Enum.IsDefined(typeof(TicketStatus), data.Status.Value),
            400,
            "Некорректный статус.");

        Require(
            !data.Priority.HasValue ||
            Enum.IsDefined(typeof(TicketPriority), data.Priority.Value),
            400,
            "Некорректный приоритет.");

        var query = db.Tickets
            .AsNoTracking()
            .Where(item => item.ProjectId == projectId);

        query = data.Archived
            ? query.Where(item => item.ArchivedAt != null)
            : query.Where(item => item.ArchivedAt == null);

        if (data.Status.HasValue)
        {
            query = query.Where(item =>
                item.Status == data.Status.Value);
        }

        if (data.Priority.HasValue)
        {
            query = query.Where(item =>
                item.Priority == data.Priority.Value);
        }

        if (data.SoftwareId.HasValue)
        {
            query = query.Where(item =>
                item.SoftwareId == data.SoftwareId.Value);
        }

        if (data.AssignedToId.HasValue)
        {
            query = query.Where(item =>
                item.Assignments.Any(assignment =>
                    assignment.IsActive &&
                    assignment.AssignedToId == data.AssignedToId.Value));
        }

        if (data.Mine)
        {
            query = query.Where(item =>
                item.Assignments.Any(assignment =>
                    assignment.IsActive &&
                    assignment.AssignedToId == userId));
        }

        if (data.Plan)
        {
            query = query.Where(item =>
                item.Deadline != null &&
                item.Assignments.Any(assignment => assignment.IsActive));
        }

        if ((data.Mine || data.Plan) && !data.Status.HasValue)
        {
            query = query.Where(item =>
                item.Status != TicketStatus.Closed);
        }

        if (!string.IsNullOrWhiteSpace(data.Search))
        {
            var search = data.Search.Trim().ToLowerInvariant();

            query = query.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Code.ToLower().Contains(search));
        }

        var total = await query.CountAsync();

        var offset = ((long)data.Page - 1) * data.PageSize;

        Require(
            offset <= int.MaxValue,
            400,
            "Слишком большой номер страницы.");

        var ordered = data.Plan
            ? query.OrderBy(item => item.Deadline)
                .ThenBy(item => item.Id)
            : query.OrderByDescending(item => item.CreatedAt)
                .ThenBy(item => item.Id);

        return new PagedTicketsResponse
        {
            Total = total,
            Page = data.Page,
            PageSize = data.PageSize,
            Items = await ordered
                .Skip((int)offset)
                .Take(data.PageSize)
                .Select(ResponseProjection)
                .ToListAsync()
        };
    }

    public async Task<TicketResponse> GetByIdAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        await RequireMemberAsync(userId, projectId);

        return await ReadResponseAsync(projectId, ticketId);
    }

    public async Task<TicketResponse> UpdateAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        UpdateTicketRequest data)
    {
        await MutateAsync(
            userId,
            projectId,
            ticketId,
            async (member, ticket) =>
            {
                RequireEditable(ticket);
                RequireReporter(member);

                Require(
                    member.Role == ProjectRole.Manager ||
                    ticket.CreatorId == userId,
                    403,
                    "Тестировщик может редактировать только свой тикет.");

                Require(
                    ticket.Status == TicketStatus.New,
                    409,
                    "Основные поля можно редактировать только в статусе New.");

                await ValidateSoftwareAsync(projectId, data.SoftwareId);

                ValidatePriority(data.Priority);

                ApplyFields(ticket, data);
            });

        return await ReadResponseAsync(projectId, ticketId);
    }

    public async Task<TicketResponse> PlanAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        PlanTicketRequest data)
    {
        await MutateAsync(
            userId,
            projectId,
            ticketId,
            async (member, ticket) =>
            {
                RequireManager(member);
                RequireEditable(ticket);

                Require(
                    ticket.Status != TicketStatus.UnderReview,
                    409,
                    "Нельзя переназначать тикет во время проверки.");

                Require(
                    data.AssignedToId.HasValue &&
                    data.AssignedToId.Value != Guid.Empty,
                    400,
                    "Укажите разработчика.");

                Require(
                    data.Deadline.HasValue &&
                    data.Deadline.Value.Kind == DateTimeKind.Utc,
                    400,
                    "Передайте срок в UTC, например 2027-01-15T12:00:00Z.");

                Require(
                    data.Deadline.Value > DateTime.UtcNow,
                    400,
                    "Срок должен быть в будущем.");

                Require(
                    !string.IsNullOrWhiteSpace(data.PlanningReason),
                    400,
                    "Укажите основание планирования.");

                var developerExists = await db.ProjectMembers.AnyAsync(
                    item =>
                        item.ProjectId == projectId &&
                        item.UserId == data.AssignedToId.Value &&
                        item.Role == ProjectRole.Developer);

                Require(
                    developerExists,
                    400,
                    "Исполнитель должен быть разработчиком этого проекта.");

                var active = ticket.Assignments
                    .SingleOrDefault(item => item.IsActive);

                if (active == null ||
                    active.AssignedToId != data.AssignedToId.Value)
                {
                    if (active != null)
                    {
                        active.IsActive = false;

                        await db.SaveChangesAsync();
                    }

                    active = new TicketAssignment
                    {
                        Id = Guid.NewGuid(),
                        TicketId = ticket.Id,
                        AssignedById = userId,
                        AssignedToId = data.AssignedToId.Value,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    db.TicketAssignments.Add(active);
                }

                ticket.Deadline = data.Deadline.Value;
                ticket.PlanningReason = data.PlanningReason.Trim();

                db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    Type = "TicketPlanned",
                    Text = $"Вам назначен тикет {ticket.Code}. Срок: {ticket.Deadline:O}",
                    RecipientId = data.AssignedToId.Value,
                    AssignmentId = active.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            });

        return await ReadResponseAsync(projectId, ticketId);
    }

    public async Task<TicketResponse> ChangeStatusAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        ChangeTicketStatusRequest data)
    {
        await MutateAsync(
            userId,
            projectId,
            ticketId,
            (member, ticket) =>
            {
                RequireEditable(ticket);

                Require(
                    data.Status.HasValue &&
                    Enum.IsDefined(typeof(TicketStatus), data.Status.Value),
                    400,
                    "Укажите корректный статус.");

                var next = data.Status.Value;
                var old = ticket.Status;
                var allowed = (old, next) switch
                {
                    (TicketStatus.New, TicketStatus.InProgress) => true,

                    (TicketStatus.InProgress, TicketStatus.UnderReview) => true,

                    (TicketStatus.UnderRevision, TicketStatus.InProgress) => true,

                    (TicketStatus.UnderRevision, TicketStatus.UnderReview) => true,

                    (TicketStatus.UnderReview, TicketStatus.Closed) => true,

                    (TicketStatus.UnderReview, TicketStatus.UnderRevision) => true,

                    _ => false
                };

                Require(
                    allowed,
                    409,
                    $"Переход {old} → {next} недоступен.");

                var active = ticket.Assignments
                    .SingleOrDefault(item => item.IsActive);

                if (next == TicketStatus.InProgress ||
                    next == TicketStatus.UnderReview)
                {
                    Require(
                        member.Role == ProjectRole.Developer &&
                        active?.AssignedToId == userId,
                        403,
                        "Это действие доступно только назначенному разработчику.");
                }
                else
                {
                    RequireReporter(member);

                    Require(
                        active?.AssignedToId != userId,
                        403,
                        "Исполнитель не может проверять собственное исправление.");
                }

                if (next == TicketStatus.UnderReview)
                {
                    Require(
                        !string.IsNullOrWhiteSpace(data.FixDescription),
                        400,
                        "Для отправки на проверку опишите исправление.");

                    Require(
                        string.IsNullOrWhiteSpace(data.CommitUrl) ||
                        IsHttpUrl(data.CommitUrl),
                        400,
                        "Ссылка на коммит должна использовать HTTP или HTTPS.");

                    ticket.FixDescription = data.FixDescription!.Trim();
                    ticket.CommitUrl = Clean(data.CommitUrl);
                    ticket.TestBuild = Clean(data.TestBuild);
                    ticket.FixRelease = Clean(data.FixRelease);
                }

                if (next == TicketStatus.UnderRevision)
                {
                    Require(
                        !string.IsNullOrWhiteSpace(data.ReturnReason),
                        400,
                        "Для возврата на доработку укажите причину.");

                    ticket.ReturnReason = data.ReturnReason!.Trim();
                }

                if (next == TicketStatus.Closed &&
                    !string.IsNullOrWhiteSpace(data.FixRelease))
                {
                    ticket.FixRelease = data.FixRelease.Trim();
                }

                ticket.Status = next;

                db.TicketStatusHistories.Add(new TicketStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticket.Id,
                    EditorId = userId,
                    OldStatus = old,
                    NewStatus = next,
                    ChangedAt = DateTime.UtcNow
                });

                return Task.CompletedTask;
            });

        return await ReadResponseAsync(projectId, ticketId);
    }


    public Task ArchiveAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        return MutateAsync(
            userId,
            projectId,
            ticketId,
            (member, ticket) =>
            {
                RequireManager(member);

                Require(
                    ticket.Status == TicketStatus.Closed,
                    409,
                    "Архивировать можно только закрытый тикет.");

                ticket.ArchivedAt ??= DateTime.UtcNow;

                return Task.CompletedTask;
            });
    }

    public Task RestoreAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        return MutateAsync(
            userId,
            projectId,
            ticketId,
            (member, ticket) =>
            {
                RequireManager(member);

                ticket.ArchivedAt = null;

                return Task.CompletedTask;
            });
    }
    public async Task<List<TicketStatusHistoryResponse>> GetHistoryAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        await RequireReadableTicketAsync(userId, projectId, ticketId);

        return await db.TicketStatusHistories
            .AsNoTracking()
            .Where(item => item.TicketId == ticketId)
            .OrderBy(item => item.ChangedAt)
            .ThenBy(item => item.Id)
            .Select(item => new TicketStatusHistoryResponse
            {
                Id = item.Id,
                OldStatus = item.OldStatus,
                NewStatus = item.NewStatus,
                ChangedAt = item.ChangedAt,
                EditorId = item.EditorId,
                EditorName = item.Editor.Name
            })
            .ToListAsync();
    }


    public async Task<List<TicketAssignmentResponse>> GetAssignmentsAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        await RequireReadableTicketAsync(userId, projectId, ticketId);

        return await db.TicketAssignments
            .AsNoTracking()
            .Where(item => item.TicketId == ticketId)
            .OrderByDescending(item => item.AssignedAt)
            .ThenBy(item => item.Id)
            .Select(item => new TicketAssignmentResponse
            {
                Id = item.Id,
                AssignedAt = item.AssignedAt,
                IsActive = item.IsActive,
                AssignedById = item.AssignedById,
                AssignedByName = item.AssignedBy.Name,
                AssignedToId = item.AssignedToId,
                AssignedToName = item.AssignedTo.Name
            })
            .ToListAsync();
    }

    public async Task<List<TicketAttachmentResponse>> GetAttachmentsAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        await RequireReadableTicketAsync(userId, projectId, ticketId);

        return await db.Attachments
            .AsNoTracking()
            .Where(item => item.TicketId == ticketId)
            .OrderBy(item => item.FileName)
            .ThenBy(item => item.Id)
            .Select(item => new TicketAttachmentResponse
            {
                Id = item.Id,
                FileName = item.FileName,
                Type = item.Type,
                Link = item.Link
            })
            .ToListAsync();
    }

    public async Task<TicketAttachmentResponse> AddAttachmentAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        AddTicketAttachmentRequest data)
    {
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            FileName = data.FileName.Trim(),
            Type = data.Type.Trim(),
            Link = data.Link.Trim()
        };


        await MutateAsync(
            userId,
            projectId,
            ticketId,
            (member, ticket) =>
            {
                RequireEditable(ticket);
                Require(
                    IsHttpUrl(attachment.Link),
                    400,
                    "Ссылка на вложение должна использовать HTTP или HTTPS.");

                db.Attachments.Add(attachment);

                return Task.CompletedTask;
            });

        return new TicketAttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            Type = attachment.Type,
            Link = attachment.Link
        };
    }

    public Task RemoveAttachmentAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        Guid attachmentId)
    {
        return MutateAsync(
            userId,
            projectId,
            ticketId,
            async (member, ticket) =>
            {
                RequireEditable(ticket);

                Require(
                    member.Role == ProjectRole.Manager ||
                    ticket.CreatorId == userId,
                    403,
                    "Удалять вложения может менеджер или автор тикета.");

                var attachment = await db.Attachments
                    .SingleOrDefaultAsync(item =>
                        item.TicketId == ticketId &&
                        item.Id == attachmentId);

                Require(
                    attachment != null,
                    404,
                    "Вложение не найдено.");

                db.Attachments.Remove(attachment!);
            });
    }


    private async Task MutateAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        Func<ProjectMember, Ticket, Task> action)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        await LockProjectAsync(projectId);

        var member = await RequireMemberAsync(userId, projectId);

        var ticket = await db.Tickets
            .Include(item => item.Assignments)
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.Id == ticketId);

        Require(
            ticket != null,
            404,
            "Тикет не найден.");

        await action(member, ticket!);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task LockProjectAsync(Guid projectId)
    {
        var projects = await db.Projects
            .FromSqlInterpolated(
                $"SELECT * FROM projects WHERE \"Id\" = {projectId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync();

        Require(
            projects.Count != 0,
            404,
            "Проект не найден.");
    }

    private async Task<ProjectMember> RequireMemberAsync(
        Guid userId,
        Guid projectId)
    {
        var member = await db.ProjectMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.UserId == userId);

        Require(
            member != null,
            404,
            "Проект не найден.");

        return member!;
    }

    private async Task RequireReadableTicketAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId)
    {
        await RequireMemberAsync(userId, projectId);

        var exists = await db.Tickets.AnyAsync(item =>
            item.ProjectId == projectId &&
            item.Id == ticketId);

        Require(
            exists,
            404,
            "Тикет не найден.");
    }

    private async Task ValidateSoftwareAsync(
        Guid projectId,
        Guid? softwareId)
    {
        Require(
            softwareId.HasValue &&
            softwareId.Value != Guid.Empty,
            400,
            "Укажите ПО.");

        var exists = await db.SoftwareItems.AnyAsync(item =>
            item.ProjectId == projectId &&
            item.Id == softwareId.Value);

        Require(
            exists,
            400,
            "ПО не найдено в этом проекте.");
    }

    private static void ValidatePriority(TicketPriority? priority)
    {
        Require(
            priority.HasValue &&
            Enum.IsDefined(typeof(TicketPriority), priority.Value),
            400,
            "Укажите корректный приоритет.");
    }

    private static void ApplyFields(
        Ticket ticket,
        CreateTicketRequest data)
    {
        ticket.Name = data.Name.Trim();
        ticket.Description = data.Description.Trim();
        ticket.ReproductionSteps = data.ReproductionSteps.Trim();
        ticket.ExpectedResult = data.ExpectedResult.Trim();
        ticket.ActualResult = data.ActualResult.Trim();
        ticket.AffectedVersion = data.AffectedVersion.Trim();
        ticket.Priority = data.Priority!.Value;
        ticket.SoftwareId = data.SoftwareId!.Value;
    }

    private static void RequireEditable(Ticket ticket)
    {
        Require(
            ticket.ArchivedAt == null,
            409,
            "Архивный тикет нельзя изменять.");

        Require(
            ticket.Status != TicketStatus.Closed,
            409,
            "Закрытый тикет нельзя изменять.");
    }

    private static void RequireManager(ProjectMember member)
    {
        Require(
            member.Role == ProjectRole.Manager,
            403,
            "Действие доступно только менеджеру.");
    }

    private static void RequireReporter(ProjectMember member)
    {
        Require(
            member.Role == ProjectRole.Manager ||
            member.Role == ProjectRole.Tester,
            403,
            "Действие доступно менеджеру или тестировщику.");
    }

    private static void Require(
        [DoesNotReturnIf(false)] bool condition,
        int statusCode,
        string message)
    {
        if (!condition)
        {
            throw new TicketOperationException(statusCode, message);
        }
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(
                value.Trim(),
                UriKind.Absolute,
                out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp ||
             uri.Scheme == Uri.UriSchemeHttps);
    }

    private async Task<TicketResponse> ReadResponseAsync(
        Guid projectId,
        Guid ticketId)
    {
        var response = await db.Tickets
            .AsNoTracking()
            .Where(item =>
                item.ProjectId == projectId &&
                item.Id == ticketId)
            .Select(ResponseProjection)
            .SingleOrDefaultAsync();

        Require(
            response != null,
            404,
            "Тикет не найден.");

        return response!;
    }

    private static readonly Expression<Func<Ticket, TicketResponse>>
        ResponseProjection = ticket => new TicketResponse
        {
            Id = ticket.Id,
            Code = ticket.Code,
            Name = ticket.Name,
            Description = ticket.Description,
            ReproductionSteps = ticket.ReproductionSteps,
            ExpectedResult = ticket.ExpectedResult,
            ActualResult = ticket.ActualResult,
            AffectedVersion = ticket.AffectedVersion,
            Priority = ticket.Priority,
            Status = ticket.Status,
            Deadline = ticket.Deadline,
            CreatedAt = ticket.CreatedAt,
            ArchivedAt = ticket.ArchivedAt,
            ProjectId = ticket.ProjectId,
            SoftwareId = ticket.SoftwareId,
            SoftwareName = ticket.Software.Name,
            CreatorId = ticket.CreatorId,
            CreatorName = ticket.Creator.Name,
            AssignedToId = ticket.Assignments
                .Where(item => item.IsActive)
                .Select(item => (Guid?)item.AssignedToId)
                .FirstOrDefault(),
            AssignedToName = ticket.Assignments
                .Where(item => item.IsActive)
                .Select(item => item.AssignedTo.Name)
                .FirstOrDefault(),
            PlanningReason = ticket.PlanningReason,
            FixDescription = ticket.FixDescription,
            CommitUrl = ticket.CommitUrl,
            TestBuild = ticket.TestBuild,
            FixRelease = ticket.FixRelease,
            ReturnReason = ticket.ReturnReason
        };
}