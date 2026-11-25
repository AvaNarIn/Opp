namespace Bugtrack.Api.Classes;

public class TicketAssignment
{
    public Guid Id { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }

    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid AssignedById { get; set; }
    public User AssignedBy { get; set; } = null!;

    public Guid AssignedToId { get; set; }
    public User AssignedTo { get; set; } = null!;
}