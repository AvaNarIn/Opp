using Bugtrack.Api.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateTicketRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(10000)]
    public string Description { get; set; } = "";

    [Required, StringLength(10000)]
    [JsonPropertyName("reproduction_steps")]
    public string ReproductionSteps { get; set; } = "";

    [Required, StringLength(5000)]
    [JsonPropertyName("expected_result")]
    public string ExpectedResult { get; set; } = "";

    [Required, StringLength(5000)]
    [JsonPropertyName("actual_result")]
    public string ActualResult { get; set; } = "";

    [Required, StringLength(100)]
    [JsonPropertyName("affected_version")]
    public string AffectedVersion { get; set; } = "";

    [Required, EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; set; }

    [Required]
    [JsonPropertyName("software_id")]
    public Guid? SoftwareId { get; set; }
}