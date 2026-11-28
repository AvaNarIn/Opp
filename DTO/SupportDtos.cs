using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateSupportRequest
{
    [Required]
    [StringLength(10000)]
    public string Text { get; set; } = "";
}

public class SupportResponse
{
    public Guid Id { get; set; }

    public string Text { get; set; } = "";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("author_id")]
    public Guid AuthorId { get; set; }

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = "";
}

public class SupportMessagesResponseDto
{
    public PaginationDto Pagination { get; set; } = new();

    public List<SupportResponse> Messages { get; set; } = new();
}