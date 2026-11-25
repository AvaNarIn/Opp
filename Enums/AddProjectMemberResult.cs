namespace Bugtrack.Api.Enums;

public enum AddProjectMemberResult
{
    Success,
    ProjectNotFound,
    Forbidden,
    UserNotFound,
    AlreadyMember,
    InvalidRole
}