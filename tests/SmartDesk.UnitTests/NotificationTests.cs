using SmartDesk.Domain.Entities;

namespace SmartDesk.UnitTests;

public class NotificationTests
{
    [Fact]
    public void MarkRead_ShouldSetReadStateWithoutChangingIdentity()
    {
        var notification = Notification.Create(Guid.NewGuid(), "TicketAssigned", "A ticket was assigned to you.");
        var id = notification.Id;

        notification.MarkRead();

        Assert.True(notification.IsRead);
        Assert.Equal(id, notification.Id);
    }
}
