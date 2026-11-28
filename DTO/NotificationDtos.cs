using System.Text.Json.Serialization;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("project_id")]
    public Guid? ProjectId { get; set; }

    [JsonPropertyName("ticket_id")]
    public Guid? TicketId { get; set; }

    [JsonPropertyName("support_message_id")]
    public Guid? SupportMessageId { get; set; }
}

public class NotificationsResponseDto
{
    public PaginationDto Pagination { get; set; } = new();

    public List<NotificationResponse> Notifications { get; set; } = new();
}