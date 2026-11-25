using Bugtrack.Api.Enums;

namespace Bugtrack.Api.Classes;

public class TicketStatusHistory
{
    public Guid Id { get; set; }

    public TicketStatus OldStatus { get; set; }
    public TicketStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid EditorId { get; set; }
    public User Editor { get; set; } = null!;
}