namespace Bugtrack.Api.Classes;

public class Attachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = "";
    public string Type { get; set; } = "";
    public string Link { get; set; } = "";

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
}