using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Ai;

namespace SmartDesk.UnitTests;

public class TicketClassificationTests
{
    [Fact]
    public async Task ClassifyAsync_WithProductionOutage_ShouldIdentifyInfrastructureAndCriticalPriority()
    {
        var classifier = new MlNetTicketClassificationService();

        var result = await classifier.ClassifyAsync("Production server unavailable", "The production service is down and all users are blocked.");

        Assert.Equal("Infrastructure", result.Category);
        Assert.Equal(TicketPriority.Critical, result.Priority);
        Assert.InRange(result.Confidence, 0m, 1m);
    }

    [Fact]
    public async Task ClassifyAsync_WithPasswordIssue_ShouldIdentifyAccountAccess()
    {
        var classifier = new MlNetTicketClassificationService();

        var result = await classifier.ClassifyAsync("Locked out", "I forgot my corporate password and cannot sign in.");

        Assert.Equal("Account Access", result.Category);
    }
}
