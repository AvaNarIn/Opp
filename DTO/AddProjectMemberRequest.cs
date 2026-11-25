using Bugtrack.Api.Enums;
using System.ComponentModel.DataAnnotations;

public class AddProjectMemberRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [EnumDataType(typeof(ProjectRole))]
    public ProjectRole? Role { get; set; }
}