using Bugtrack.Api.Enums;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

public class TicketQueryRequest
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; set; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; set; }

    [StringLength(200)]
    public string? Search { get; set; }

    [FromQuery(Name = "software_id")]
    public Guid? SoftwareId { get; set; }

    [FromQuery(Name = "assigned_to_id")]
    public Guid? AssignedToId { get; set; }

    public bool Mine { get; set; }

    public bool Plan { get; set; }

    public bool Archived { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    [FromQuery(Name = "page_size")]
    public int PageSize { get; set; } = 20;
}