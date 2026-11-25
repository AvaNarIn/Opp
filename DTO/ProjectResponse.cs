using Bugtrack.Api.Enums;
using System.Text.Json.Serialization;

public class ProjectResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string Link { get; set; } = "";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    public ProjectRole Role { get; set; }
}