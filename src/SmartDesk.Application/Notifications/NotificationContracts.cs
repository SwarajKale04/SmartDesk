namespace SmartDesk.Application.Notifications;

public sealed record NotificationDto(Guid Id, string Type, string Message, Guid? RelatedTicketId, bool IsRead, DateTimeOffset CreatedAt);

public interface INotificationService
{
    Task NotifyAsync(Guid userId, string type, string message, Guid? relatedTicketId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}
