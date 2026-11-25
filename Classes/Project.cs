using Bugtrack.Api.Enums;

namespace Bugtrack.Api.Classes;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public List<Software> SoftwareItems { get; set; } = new();
    public List<ProjectMember> Members { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();

    public string Description { get; set; } = "";
    public string Link { get; set; } = "";

    public void ChangeName(string name)
    {
        Name = name;
    }

    public void AddSoftware(Software software)
    {
        software.ProjectId = Id;
        software.Project = this;

        SoftwareItems.Add(software);
    }

    public void AddUser(User user, ProjectRole role)
    {
        var member = new ProjectMember
        {
            ProjectId = Id,
            Project = this,
            UserId = user.Id,
            User = user,
            Role = role
        };

        Members.Add(member);
    }
    public void AddTicket(Ticket ticket)
{
    ticket.ProjectId = Id;
    ticket.Project = this;

    Tickets.Add(ticket);
}

    public void ChangeRole(User user, ProjectRole role)
    {
        var member = Members.Single(item => item.UserId == user.Id);
        member.Role = role;
    }

    public void RemoveUser(User user)
    {
        var member = Members.Single(item => item.UserId == user.Id);
        Members.Remove(member);
    }

    public void RemoveSoftware(Software software)
    {
        SoftwareItems.Remove(software);
    }
}