using SmartDesk.Domain.Entities;
using SmartDesk.Domain.Enums;

namespace SmartDesk.UnitTests;

public class TicketDefaultsTests
{
    [Fact]
    public void NewTicket_ShouldStartInNewStateWithMediumPriorityAndOnTrackSla()
    {
        var ticket = new Ticket();

        Assert.Equal(TicketStatus.New, ticket.Status);
        Assert.Equal(TicketPriority.Medium, ticket.Priority);
        Assert.Equal(SlaStatus.OnTrack, ticket.SlaStatus);
        Assert.False(ticket.AiReviewRequired);
        Assert.NotEqual(Guid.Empty, ticket.Id);
    }
}
