namespace Bugtrack.Api.Classes;

public class Notification
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    public Guid? AssignmentId { get; set; }
    public TicketAssignment? Assignment { get; set; }

    public Guid? SupportMessageId { get; set; }
    public SupportMessage? SupportMessage { get; set; }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}