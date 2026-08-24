using SmartDesk.Domain.Enums;

namespace SmartDesk.Application.Sla;

public sealed record SlaCalculation(Guid PolicyId, DateTimeOffset FirstResponseDueAt, DateTimeOffset ResolutionDueAt);

public interface ISlaCalculationService
{
    Task<SlaCalculation?> CalculateAsync(TicketPriority priority, DateTimeOffset createdAt, CancellationToken cancellationToken = default);
}
