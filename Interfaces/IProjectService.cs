using Bugtrack.Api.Enums;
public interface IProjectService
{
    Task<Guid?> CreateAsync(Guid userId, CreateProjectRequest data);
    Task<List<ProjectResponse>> GetMyProjectsAsync(Guid userId);
    Task<ProjectResponse?> GetByIdAsync(Guid userId, Guid projectId);
    Task<List<ProjectMemberResponse>?> GetMembersAsync(Guid userId, Guid projectId);

    Task<AddProjectMemberResult> AddMemberAsync(Guid userId, Guid projectId, AddProjectMemberRequest data);

    Task<ChangeProjectMemberResult> ChangeMemberRoleAsync(Guid userId, Guid projectId, Guid memberId, ChangeProjectMemberRoleRequest data);

    Task<ChangeProjectMemberResult> RemoveMemberAsync(Guid userId, Guid projectId, Guid memberId);
}