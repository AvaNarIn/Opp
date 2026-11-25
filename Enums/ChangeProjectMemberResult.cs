namespace Bugtrack.Api.Enums;

public enum ChangeProjectMemberResult
{
    Success,
    ProjectNotFound,
    Forbidden,
    MemberNotFound,
    LastManager,
    InvalidRole
}