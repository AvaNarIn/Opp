using Bugtrack.Api.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UpdateTicketRequest : CreateTicketRequest
{
}

public class PlanTicketRequest
{
    [Required]
    [JsonPropertyName("assigned_to_id")]
    public Guid? AssignedToId { get; set; }

    [Required]
    public DateTime? Deadline { get; set; }

    [Required, StringLength(2000)]
    [JsonPropertyName("planning_reason")]
    public string PlanningReason { get; set; } = "";
}

public class ChangeTicketStatusRequest
{
    [Required, EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; set; }

    [StringLength(5000)]
    [JsonPropertyName("fix_description")]
    public string? FixDescription { get; set; }

    [Url, StringLength(1000)]
    [JsonPropertyName("commit_url")]
    public string? CommitUrl { get; set; }

    [StringLength(1000)]
    [JsonPropertyName("test_build")]
    public string? TestBuild { get; set; }

    [StringLength(100)]
    [JsonPropertyName("fix_release")]
    public string? FixRelease { get; set; }

    [StringLength(5000)]
    [JsonPropertyName("return_reason")]
    public string? ReturnReason { get; set; }
}

public class AddTicketAttachmentRequest
{
    [Required, StringLength(255)]
    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = "";

    [Required, StringLength(100)]
    public string Type { get; set; } = "";

    [Required, Url, StringLength(2000)]
    public string Link { get; set; } = "";
}