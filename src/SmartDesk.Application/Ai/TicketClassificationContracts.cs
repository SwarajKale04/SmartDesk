using SmartDesk.Domain.Enums;

namespace SmartDesk.Application.Ai;

public sealed record TicketClassification(string Category, TicketPriority Priority, decimal Confidence);

public interface ITicketClassificationService
{
    Task<TicketClassification> ClassifyAsync(string title, string description, CancellationToken cancellationToken = default);
}
