using Bugtrack.Api.Enums;
using System.ComponentModel.DataAnnotations;

public class ChangeProjectMemberRoleRequest
{
    [Required]
    [EnumDataType(typeof(ProjectRole))]
    public ProjectRole? Role { get; set; }
}