using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Microsoft.EntityFrameworkCore;

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
}