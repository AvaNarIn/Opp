using Bugtrack.Api.Classes;
using Microsoft.EntityFrameworkCore;

namespace Bugtrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Software> SoftwareItems => Set<Software>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<TicketAssignment> TicketAssignments =>
        Set<TicketAssignment>();
    public DbSet<TicketStatusHistory> TicketStatusHistories =>
        Set<TicketStatusHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        user.ToTable("users");
        user.HasKey(item => item.Id);

        user.Property(item => item.Name).IsRequired();
        user.Property(item => item.Email).IsRequired();
        user.Property(item => item.PasswordHash).IsRequired();

        user.Property(item => item.SystemRole)
            .HasConversion<string>()
            .IsRequired();

        user.HasIndex(item => item.Email)
            .IsUnique()
            .HasDatabaseName("UX_users_Email");

        var project = modelBuilder.Entity<Project>();

        project.ToTable("projects");
        project.HasKey(item => item.Id);

        project.Property(item => item.Name).IsRequired();
        project.Property(item => item.Description).IsRequired();
        project.Property(item => item.Link).IsRequired();

        var software = modelBuilder.Entity<Software>();

        software.ToTable("software");
        software.HasKey(item => item.Id);

        software.Property(item => item.Name).IsRequired();
        software.Property(item => item.Version).IsRequired();
        software.Property(item => item.Link).IsRequired();

        software.HasAlternateKey(item => new
        {
            item.ProjectId,
            item.Id
        });

        software.HasOne(item => item.Project)
            .WithMany(item => item.SoftwareItems)
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        var member = modelBuilder.Entity<ProjectMember>();

        member.ToTable("project_members");

        member.HasKey(item => new
        {
            item.ProjectId,
            item.UserId
        });

        member.Property(item => item.Role)
            .HasConversion<string>()
            .IsRequired();

        member.HasOne(item => item.Project)
            .WithMany(item => item.Members)
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        member.HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        var ticket = modelBuilder.Entity<Ticket>();

        ticket.ToTable("tickets");
        ticket.HasKey(item => item.Id);

        ticket.Property(item => item.Code).IsRequired();
        ticket.Property(item => item.Name).IsRequired();
        ticket.Property(item => item.Description).IsRequired();
        ticket.Property(item => item.ReproductionSteps).IsRequired();
        ticket.Property(item => item.ExpectedResult).IsRequired();
        ticket.Property(item => item.ActualResult).IsRequired();
        ticket.Property(item => item.AffectedVersion).IsRequired();

        ticket.Property(item => item.Status)
            .HasConversion<string>()
            .IsRequired();

        ticket.Property(item => item.Priority)
            .HasConversion<string>()
            .IsRequired();

        ticket.HasIndex(item => new
        {
            item.ProjectId,
            item.Code
        }).IsUnique();

        ticket.HasOne(item => item.Project)
            .WithMany(item => item.Tickets)
            .HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasOne(item => item.Software)
            .WithMany()
            .HasForeignKey(item => new
            {
                item.ProjectId,
                item.SoftwareId
            })
            .HasPrincipalKey(item => new
            {
                item.ProjectId,
                item.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasOne(item => item.Creator)
            .WithMany()
            .HasForeignKey(item => item.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        var attachment = modelBuilder.Entity<Attachment>();

        attachment.ToTable("attachments");
        attachment.HasKey(item => item.Id);

        attachment.Property(item => item.FileName).IsRequired();
        attachment.Property(item => item.Type).IsRequired();
        attachment.Property(item => item.Link).IsRequired();

        attachment.HasOne(item => item.Ticket)
            .WithMany(item => item.Attachments)
            .HasForeignKey(item => item.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        var assignment = modelBuilder.Entity<TicketAssignment>();

        assignment.ToTable("ticket_assignments");
        assignment.HasKey(item => item.Id);

        assignment.HasIndex(item => item.TicketId)
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE");

        assignment.HasOne(item => item.Ticket)
            .WithMany(item => item.Assignments)
            .HasForeignKey(item => item.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        assignment.HasOne(item => item.AssignedBy)
            .WithMany()
            .HasForeignKey(item => item.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        assignment.HasOne(item => item.AssignedTo)
            .WithMany()
            .HasForeignKey(item => item.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        var history = modelBuilder.Entity<TicketStatusHistory>();

        history.ToTable("ticket_status_history");
        history.HasKey(item => item.Id);

        history.Property(item => item.OldStatus)
            .HasConversion<string>()
            .IsRequired();

        history.Property(item => item.NewStatus)
            .HasConversion<string>()
            .IsRequired();

        history.HasOne(item => item.Ticket)
            .WithMany(item => item.StatusHistory)
            .HasForeignKey(item => item.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        history.HasOne(item => item.Editor)
            .WithMany()
            .HasForeignKey(item => item.EditorId)
            .OnDelete(DeleteBehavior.Restrict);

        var message = modelBuilder.Entity<SupportMessage>();

        message.ToTable("support_messages");
        message.HasKey(item => item.Id);

        message.Property(item => item.Text).IsRequired();

        message.HasOne(item => item.Author)
            .WithMany()
            .HasForeignKey(item => item.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        var notification = modelBuilder.Entity<Notification>();

        notification.ToTable("notifications");
        notification.HasKey(item => item.Id);

        notification.Property(item => item.Type).IsRequired();
        notification.Property(item => item.Text).IsRequired();

        notification.HasOne(item => item.Recipient)
            .WithMany()
            .HasForeignKey(item => item.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        notification.HasOne(item => item.Assignment)
            .WithMany()
            .HasForeignKey(item => item.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        notification.HasOne(item => item.SupportMessage)
            .WithMany()
            .HasForeignKey(item => item.SupportMessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}