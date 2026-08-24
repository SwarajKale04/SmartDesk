using Microsoft.EntityFrameworkCore;
using SmartDesk.Application.Sla;
using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Persistence;

namespace SmartDesk.Infrastructure.Sla;

public sealed class SlaCalculationService(SmartDeskDbContext dbContext) : ISlaCalculationService
{
    public async Task<SlaCalculation?> CalculateAsync(TicketPriority priority, DateTimeOffset createdAt, CancellationToken cancellationToken = default)
    {
        var policy = await dbContext.SlaPolicies.AsNoTracking().SingleOrDefaultAsync(x => x.Priority == priority && x.IsActive, cancellationToken);
        return policy is null ? null : new SlaCalculation(policy.Id, createdAt.AddMinutes(policy.ResponseTimeMinutes), createdAt.AddMinutes(policy.ResolutionTimeMinutes));
    }
}
