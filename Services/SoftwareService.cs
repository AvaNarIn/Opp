using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Microsoft.EntityFrameworkCore;
using Bugtrack.Api.Enums;
public class SoftwareService : ISoftwareService
{
    private readonly AppDbContext db;

    public SoftwareService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<SoftwareResponse> CreateAsync(
        Guid projectId,
        CreateSoftwareRequest data)
    {
        var software = new Software
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = data.Name.Trim(),
            Version = data.Version.Trim(),
            Link = data.Link.Trim()
        };

        db.SoftwareItems.Add(software);
        await db.SaveChangesAsync();

        return new SoftwareResponse
        {
            Id = software.Id,
            Name = software.Name,
            Version = software.Version,
            Link = software.Link
        };
    }

    public async Task<List<SoftwareResponse>> GetByProjectAsync(
        Guid projectId)
    {
        return await db.SoftwareItems
            .AsNoTracking()
            .Where(software => software.ProjectId == projectId)
            .OrderBy(software => software.Name)
            .ThenBy(software => software.Version)
            .Select(software => new SoftwareResponse
            {
                Id = software.Id,
                Name = software.Name,
                Version = software.Version,
                Link = software.Link
            })
            .ToListAsync();
    }


    public async Task<SoftwareResponse> UpdateAsync(
        Guid userId,
        Guid projectId,
        Guid softwareId,
        UpdateSoftwareRequest data)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        await CheckManagerAsync(userId, projectId);

        var software = await db.SoftwareItems
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.Id == softwareId);

        if (software == null)
        {
            throw new TicketOperationException(
                404,
                "ПО не найдено.");
        }

        software.Name = data.Name.Trim();
        software.Version = data.Version.Trim();
        software.Link = data.Link.Trim();

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new SoftwareResponse
        {
            Id = software.Id,
            Name = software.Name,
            Version = software.Version,
            Link = software.Link
        };
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid projectId,
        Guid softwareId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        await CheckManagerAsync(userId, projectId);

        var software = await db.SoftwareItems
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.Id == softwareId);

        if (software == null)
        {
            throw new TicketOperationException(
                404,
                "ПО не найдено.");
        }

        var hasTickets = await db.Tickets.AnyAsync(
            item => item.SoftwareId == softwareId);

        if (hasTickets)
        {
            throw new TicketOperationException(
                409,
                "Нельзя удалить ПО, на которое ссылаются тикеты, включая архивные.");
        }

        db.SoftwareItems.Remove(software);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task CheckManagerAsync(
        Guid userId,
        Guid projectId)
    {
        var projects = await db.Projects
            .FromSqlInterpolated(
                $"SELECT * FROM projects WHERE \"Id\" = {projectId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync();

        var member = await db.ProjectMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.UserId == userId);

        if (projects.Count == 0 || member == null)
        {
            throw new TicketOperationException(
                404,
                "Проект не найден.");
        }

        if (member.Role != ProjectRole.Manager)
        {
            throw new TicketOperationException(
                403,
                "Управлять ПО может только менеджер.");
        }
    }
}