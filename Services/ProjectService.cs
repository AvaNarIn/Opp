using Bugtrack.Api.Classes;
using Bugtrack.Api.Data;
using Bugtrack.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
public class ProjectService : IProjectService
{
    private readonly AppDbContext db;

    public ProjectService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<Guid?> CreateAsync(
        Guid userId,
        CreateProjectRequest data)
    {
        var userExists = await db.Users.AnyAsync(
            user => user.Id == userId);

        if (!userExists)
        {
            return null;
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = data.Name.Trim(),
            Description = data.Description.Trim(),
            Link = data.Link.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        project.Members.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Manager
        });

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return project.Id;
    }

    public async Task<List<ProjectResponse>> GetMyProjectsAsync(
        Guid userId)
    {
        return await db.ProjectMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .OrderByDescending(member => member.Project.CreatedAt)
            .Select(member => new ProjectResponse
            {
                Id = member.Project.Id,
                Name = member.Project.Name,
                Description = member.Project.Description,
                Link = member.Project.Link,
                CreatedAt = member.Project.CreatedAt,
                Role = member.Role
            })
            .ToListAsync();
    }


    public async Task<ProjectResponse?> GetByIdAsync(
        Guid userId,
        Guid projectId)
    {
        return await db.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                member.UserId == userId &&
                member.ProjectId == projectId)
            .Select(member => new ProjectResponse
            {
                Id = member.Project.Id,
                Name = member.Project.Name,
                Description = member.Project.Description,
                Link = member.Project.Link,
                CreatedAt = member.Project.CreatedAt,
                Role = member.Role
            })
            .SingleOrDefaultAsync();
    }

    public async Task<List<ProjectMemberResponse>?> GetMembersAsync(
    Guid userId,
    Guid projectId)
    {
        var isMember = await db.ProjectMembers.AnyAsync(member =>
            member.ProjectId == projectId &&
            member.UserId == userId);

        if (!isMember)
        {
            return null;
        }

        return await db.ProjectMembers
            .AsNoTracking()
            .Where(member => member.ProjectId == projectId)
            .OrderBy(member => member.User.Name)
            .Select(member => new ProjectMemberResponse
            {
                UserId = member.UserId,
                Name = member.User.Name,
                Email = member.User.Email,
                Role = member.Role
            })
            .ToListAsync();
    }


    public async Task<AddProjectMemberResult> AddMemberAsync(
        Guid userId,
        Guid projectId,
        AddProjectMemberRequest data)
    {
        var currentMember = await db.ProjectMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(member =>
                member.ProjectId == projectId &&
                member.UserId == userId);

        if (currentMember == null)
        {
            return AddProjectMemberResult.ProjectNotFound;
        }

        if (currentMember.Role != ProjectRole.Manager)
        {
            return AddProjectMemberResult.Forbidden;
        }

        if (data.Role == null ||
            !Enum.IsDefined(typeof(ProjectRole), data.Role.Value))
        {
            return AddProjectMemberResult.InvalidRole;
        }

        var email = data.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email);

        if (user == null)
        {
            return AddProjectMemberResult.UserNotFound;
        }

        var alreadyMember = await db.ProjectMembers.AnyAsync(member =>
            member.ProjectId == projectId &&
            member.UserId == user.Id);

        if (alreadyMember)
        {
            return AddProjectMemberResult.AlreadyMember;
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = user.Id,
            Role = data.Role.Value
        };

        db.ProjectMembers.Add(member);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException postgres &&
                postgres.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            db.Entry(member).State = EntityState.Detached;
            return AddProjectMemberResult.AlreadyMember;
        }

        return AddProjectMemberResult.Success;
    }



    public Task<ChangeProjectMemberResult> ChangeMemberRoleAsync(
        Guid userId,
        Guid projectId,
        Guid memberId,
        ChangeProjectMemberRoleRequest data)
    {
        if (data.Role == null ||
            !Enum.IsDefined(typeof(ProjectRole), data.Role.Value))
        {
            return Task.FromResult(ChangeProjectMemberResult.InvalidRole);
        }

        return ChangeMemberAsync(
            userId,
            projectId,
            memberId,
            data.Role.Value);
    }

    public Task<ChangeProjectMemberResult> RemoveMemberAsync(
        Guid userId,
        Guid projectId,
        Guid memberId)
    {
        return ChangeMemberAsync(
            userId,
            projectId,
            memberId,
            null);
    }

    private async Task<ChangeProjectMemberResult> ChangeMemberAsync(
        Guid userId,
        Guid projectId,
        Guid memberId,
        ProjectRole? newRole)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var projects = await db.Projects
            .FromSqlInterpolated(
                $"SELECT * FROM projects WHERE \"Id\" = {projectId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync();

        if (projects.Count == 0)
        {
            return ChangeProjectMemberResult.ProjectNotFound;
        }

        var currentMember = await db.ProjectMembers
            .SingleOrDefaultAsync(member =>
                member.ProjectId == projectId &&
                member.UserId == userId);

        if (currentMember == null)
        {
            return ChangeProjectMemberResult.ProjectNotFound;
        }

        if (currentMember.Role != ProjectRole.Manager)
        {
            return ChangeProjectMemberResult.Forbidden;
        }

        var targetMember = await db.ProjectMembers
            .SingleOrDefaultAsync(member =>
                member.ProjectId == projectId &&
                member.UserId == memberId);

        if (targetMember == null)
        {
            return ChangeProjectMemberResult.MemberNotFound;
        }

        if (targetMember.Role == ProjectRole.Manager &&
            newRole != ProjectRole.Manager)
        {
            var managerCount = await db.ProjectMembers.CountAsync(member =>
                member.ProjectId == projectId &&
                member.Role == ProjectRole.Manager);

            if (managerCount <= 1)
            {
                return ChangeProjectMemberResult.LastManager;
            }
        }

        if (newRole == null)
        {
            db.ProjectMembers.Remove(targetMember);
        }
        else
        {
            targetMember.Role = newRole.Value;
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return ChangeProjectMemberResult.Success;
    }

    public async Task<ProjectResponse> UpdateAsync(
        Guid userId,
        Guid projectId,
        UpdateProjectRequest data)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var projects = await db.Projects
            .FromSqlInterpolated(
                $"SELECT * FROM projects WHERE \"Id\" = {projectId} FOR UPDATE")
            .ToListAsync();

        var project = projects.SingleOrDefault();

        var member = await db.ProjectMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.ProjectId == projectId &&
                item.UserId == userId);

        if (project == null || member == null)
        {
            throw new TicketOperationException(
                404,
                "Проект не найден.");
        }

        if (member.Role != ProjectRole.Manager)
        {
            throw new TicketOperationException(
                403,
                "Редактировать проект может только менеджер.");
        }

        project.Name = data.Name.Trim();
        project.Description = data.Description.Trim();
        project.Link = data.Link.Trim();

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Link = project.Link,
            CreatedAt = project.CreatedAt,
            Role = member.Role
        };
    }
}