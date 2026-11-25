using Bugtrack.Api.Enums;
using System.Text.Json.Serialization;

public class TicketResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [JsonPropertyName("reproduction_steps")]
    public string ReproductionSteps { get; set; } = "";

    [JsonPropertyName("expected_result")]
    public string ExpectedResult { get; set; } = "";

    [JsonPropertyName("actual_result")]
    public string ActualResult { get; set; } = "";

    [JsonPropertyName("affected_version")]
    public string AffectedVersion { get; set; } = "";

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    public DateTime? Deadline { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("archived_at")]
    public DateTime? ArchivedAt { get; set; }

    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }

    [JsonPropertyName("software_id")]
    public Guid SoftwareId { get; set; }

    [JsonPropertyName("software_name")]
    public string SoftwareName { get; set; } = "";

    [JsonPropertyName("creator_id")]
    public Guid CreatorId { get; set; }

    [JsonPropertyName("creator_name")]
    public string CreatorName { get; set; } = "";

    [JsonPropertyName("assigned_to_id")]
    public Guid? AssignedToId { get; set; }

    [JsonPropertyName("assigned_to_name")]
    public string? AssignedToName { get; set; }

    [JsonPropertyName("planning_reason")]
    public string? PlanningReason { get; set; }

    [JsonPropertyName("fix_description")]
    public string? FixDescription { get; set; }

    [JsonPropertyName("commit_url")]
    public string? CommitUrl { get; set; }

    [JsonPropertyName("test_build")]
    public string? TestBuild { get; set; }

    [JsonPropertyName("fix_release")]
    public string? FixRelease { get; set; }

    [JsonPropertyName("return_reason")]
    public string? ReturnReason { get; set; }
}

public class PagedTicketsResponse
{
    public int Total { get; set; }

    public int Page { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    public List<TicketResponse> Items { get; set; } = new();
}



public class TicketOperationException : Exception
{
    public int StatusCode { get; }

    public TicketOperationException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}