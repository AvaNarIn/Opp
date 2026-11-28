public interface INotificationService
{
    Task<NotificationsResponseDto> GetAsync(Guid userId, int page, int perPage, bool? isRead);

    Task<int> GetUnreadCountAsync(Guid userId);

    Task MarkReadAsync(Guid userId, Guid notificationId);

    Task MarkAllReadAsync(Guid userId);
}