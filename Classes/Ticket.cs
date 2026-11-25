using Bugtrack.Api.Enums;

namespace Bugtrack.Api.Classes;

public class Ticket
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ReproductionSteps { get; set; } = "";

    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;

    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid SoftwareId { get; set; }
    public Software Software { get; set; } = null!;

    public Guid CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public List<Attachment> Attachments { get; set; } = new();

    public List<TicketAssignment> Assignments { get; set; } = new();

    public List<TicketStatusHistory> StatusHistory { get; set; } = new();

    public string Code { get; set; } = "";
    public string ExpectedResult { get; set; } = "";
    public string ActualResult { get; set; } = "";
    public string AffectedVersion { get; set; } = "";

    public string? PlanningReason { get; set; }

    public string? FixDescription { get; set; }
    public string? CommitUrl { get; set; }
    public string? TestBuild { get; set; }
    public string? FixRelease { get; set; }

    public string? ReturnReason { get; set; }

    public void ChangeName(string name)
    {
        Name = name;
    }

    public void ChangeDescription(string description)
    {
        Description = description;
    }

    public void ChangeReproductionSteps(string reproductionSteps)
    {
        ReproductionSteps = reproductionSteps;
    }

    public void ChangeStatus(TicketStatus status)
    {
        Status = status;
    }

    public void SetPriority(TicketPriority priority)
    {
        Priority = priority;
    }

    public void SetDeadline(DateTime? deadline)
    {
        Deadline = deadline;
    }

    public void Archive()
    {
        ArchivedAt = DateTime.UtcNow;
    }

    public void RestoreFromArchive()
    {
        ArchivedAt = null;
    }

    public void AddAttachment(Attachment attachment)
    {
        attachment.TicketId = Id;
        attachment.Ticket = this;

        Attachments.Add(attachment);
    }

    public void RemoveAttachment(Attachment attachment)
    {
        Attachments.Remove(attachment);
    }

    public void Assign(User assignedBy, User assignedTo)
    {
        foreach (var assignment in Assignments)
        {
            assignment.IsActive = false;
        }

        var newAssignment = new TicketAssignment
        {
            Id = Guid.NewGuid(),
            TicketId = Id,
            Ticket = this,
            AssignedById = assignedBy.Id,
            AssignedBy = assignedBy,
            AssignedToId = assignedTo.Id,
            AssignedTo = assignedTo,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        Assignments.Add(newAssignment);
    }
}