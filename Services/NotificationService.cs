using Bugtrack.Api.Data;
using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly AppDbContext db;

    public NotificationService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<NotificationsResponseDto> GetAsync(
        Guid userId,
        int page,
        int perPage,
        bool? isRead)
    {
        var query = db.Notifications
            .AsNoTracking()
            .Where(item => item.RecipientId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(item =>
                item.IsRead == isRead.Value);
        }

        var total = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(
            (double)total / perPage);

        var notifications = new List<NotificationResponse>();

        if (page <= totalPages)
        {
            notifications = await query
                .OrderByDescending(item => item.CreatedAt)
                .ThenBy(item => item.Id)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(item => new NotificationResponse
                {
                    Id = item.Id,
                    Type = item.Type,
                    Text = item.Text,
                    IsRead = item.IsRead,
                    CreatedAt = item.CreatedAt,
                    ProjectId = item.Assignment == null
                        ? null
                        : (Guid?)item.Assignment.Ticket.ProjectId,
                    TicketId = item.Assignment == null
                        ? null
                        : (Guid?)item.Assignment.TicketId,
                    SupportMessageId = item.SupportMessageId
                })
                .ToListAsync();
        }

        return new NotificationsResponseDto
        {
            Pagination = new PaginationDto
            {
                Total = total,
                Page = page,
                PerPage = notifications.Count,
                TotalPages = totalPages
            },
            Notifications = notifications
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await db.Notifications.CountAsync(item =>
            item.RecipientId == userId &&
            !item.IsRead);
    }

    public async Task MarkReadAsync(
        Guid userId,
        Guid notificationId)
    {
        var notification = await db.Notifications
            .SingleOrDefaultAsync(item =>
                item.Id == notificationId &&
                item.RecipientId == userId);

        if (notification == null)
        {
            throw new TicketOperationException(
                404,
                "Уведомление не найдено.");
        }

        notification.MarkAsRead();

        await db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        await db.Notifications
            .Where(item =>
                item.RecipientId == userId &&
                !item.IsRead)
            .ExecuteUpdateAsync(update =>
                update.SetProperty(item => item.IsRead, true));
    }
}