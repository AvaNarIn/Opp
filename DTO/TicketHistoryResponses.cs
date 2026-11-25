using Bugtrack.Api.Enums;
using System.Text.Json.Serialization;

public class TicketStatusHistoryResponse
{
    public Guid Id { get; set; }

    [JsonPropertyName("old_status")]
    public TicketStatus OldStatus { get; set; }

    [JsonPropertyName("new_status")]
    public TicketStatus NewStatus { get; set; }

    [JsonPropertyName("changed_at")]
    public DateTime ChangedAt { get; set; }

    [JsonPropertyName("editor_id")]
    public Guid EditorId { get; set; }

    [JsonPropertyName("editor_name")]
    public string EditorName { get; set; } = "";
}

public class TicketAssignmentResponse
{
    public Guid Id { get; set; }

    [JsonPropertyName("assigned_at")]
    public DateTime AssignedAt { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("assigned_by_id")]
    public Guid AssignedById { get; set; }

    [JsonPropertyName("assigned_by_name")]
    public string AssignedByName { get; set; } = "";

    [JsonPropertyName("assigned_to_id")]
    public Guid AssignedToId { get; set; }

    [JsonPropertyName("assigned_to_name")]
    public string AssignedToName { get; set; } = "";
}

public class TicketAttachmentResponse
{
    public Guid Id { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = "";

    public string Type { get; set; } = "";

    public string Link { get; set; } = "";
}