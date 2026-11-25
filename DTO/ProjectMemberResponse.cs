using Bugtrack.Api.Enums;
using System.Text.Json.Serialization;

public class ProjectMemberResponse
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public ProjectRole Role { get; set; }
}