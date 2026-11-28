using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Bugtrack.Api.Enums;
using Microsoft.EntityFrameworkCore;

public class AttachmentFileService : IAttachmentFileService
{
    private readonly AppDbContext db;
    private readonly AttachmentStorage storage;

    public AttachmentFileService(
        AppDbContext db,
        AttachmentStorage storage)
    {
        this.db = db;
        this.storage = storage;
    }

    public async Task<TicketAttachmentResponse> UploadAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync(cancellationToken);

        var projects = await db.Projects
            .FromSqlInterpolated(
                $"SELECT * FROM projects WHERE \"Id\" = {projectId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (projects.Count == 0)
        {
            throw new TicketOperationException(
                404,
                "Проект не найден.");
        }

        await RequireMemberAsync(
            userId,
            projectId,
            cancellationToken);

        var ticket = await db.Tickets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.ProjectId == projectId &&
                    item.Id == ticketId,
                cancellationToken);

        if (ticket == null)
        {
            throw new TicketOperationException(
                404,
                "Тикет не найден.");
        }

        if (ticket.ArchivedAt != null ||
            ticket.Status == TicketStatus.Closed)
        {
            throw new TicketOperationException(
                409,
                "Нельзя добавлять файлы в закрытый или архивный тикет.");
        }

        var metadata = AttachmentStorage.Validate(file);

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            FileName = metadata.FileName,
            Type = metadata.ContentType
        };

        attachment.Link = DownloadPath(
            projectId,
            ticketId,
            attachment.Id);

        try
        {
            await storage.SaveAsync(
                attachment.Id,
                file,
                cancellationToken);

            db.Attachments.Add(attachment);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            storage.DeleteIfExists(attachment.Id);
            throw;
        }

        return new TicketAttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            Type = attachment.Type,
            Link = attachment.Link
        };
    }

    public async Task<AttachmentDownload> DownloadAsync(
        Guid userId,
        Guid projectId,
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        await RequireMemberAsync(
            userId,
            projectId,
            cancellationToken);

        var attachment = await db.Attachments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.Id == attachmentId &&
                    item.TicketId == ticketId &&
                    item.Ticket.ProjectId == projectId,
                cancellationToken);

        if (attachment == null)
        {
            throw new TicketOperationException(
                404,
                "Вложение не найдено.");
        }

        var localLink = DownloadPath(
            projectId,
            ticketId,
            attachmentId);

        if (attachment.Link != localLink)
        {
            throw new TicketOperationException(
                409,
                "Вложение хранится по внешней ссылке, загруженного файла нет.");
        }

        return new AttachmentDownload
        {
            Content = storage.Open(attachmentId),
            ContentType = attachment.Type,
            FileName = attachment.FileName
        };
    }

    private async Task RequireMemberAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var memberExists = await db.ProjectMembers.AnyAsync(
            item =>
                item.ProjectId == projectId &&
                item.UserId == userId,
            cancellationToken);

        if (!memberExists)
        {
            throw new TicketOperationException(
                404,
                "Проект не найден.");
        }
    }

    private static string DownloadPath(
        Guid projectId,
        Guid ticketId,
        Guid attachmentId)
    {
        return $"/api/projects/{projectId}/tickets/{ticketId}/attachments/{attachmentId}/download";
    }
}