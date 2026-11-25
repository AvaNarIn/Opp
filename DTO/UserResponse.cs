using Bugtrack.Api.Enums;
using System.Text.Json.Serialization;

public class UserResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("system_role")]
    public SystemRole SystemRole { get; set; }
}