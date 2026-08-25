using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartDesk.Application.Common;
using SmartDesk.Application.Notifications;
using SmartDesk.Domain.Entities;
using SmartDesk.Infrastructure.Persistence;

namespace SmartDesk.Infrastructure.Notifications;

public sealed class NotificationService(SmartDeskDbContext dbContext, IHubContext<NotificationHub> hubContext) : INotificationService
{
    public async Task NotifyAsync(Guid userId, string type, string message, Guid? relatedTicketId = null, CancellationToken cancellationToken = default)
    {
        var notification = Notification.Create(userId, type, message, relatedTicketId);
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        await hubContext.Clients.Group($"user:{userId}").SendAsync("NotificationReceived", ToDto(notification), cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default) => await dbContext.Notifications.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).Take(50).Select(x => new NotificationDto(x.Id, x.Type, x.Message, x.RelatedTicketId, x.IsRead, x.CreatedAt)).ToListAsync(cancellationToken);

    public async Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await dbContext.Notifications.SingleOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken) ?? throw new NotFoundException("Notification not found.");
        notification.MarkRead();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto ToDto(Notification notification) => new(notification.Id, notification.Type, notification.Message, notification.RelatedTicketId, notification.IsRead, notification.CreatedAt);
}
