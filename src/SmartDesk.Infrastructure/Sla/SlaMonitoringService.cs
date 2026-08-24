using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartDesk.Domain.Entities;
using SmartDesk.Domain.Enums;
using SmartDesk.Infrastructure.Persistence;

namespace SmartDesk.Infrastructure.Sla;

public sealed class SlaMonitoringService(IServiceScopeFactory scopeFactory, IOptions<SlaMonitoringOptions> options, ILogger<SlaMonitoringService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.IntervalMinutes));
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await MonitorAsync(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "SLA monitoring iteration failed"); }
            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartDeskDbContext>();
        var now = DateTimeOffset.UtcNow;
        var threshold = now.AddMinutes(options.Value.AtRiskThresholdMinutes);
        var activeTickets = await dbContext.Tickets.Where(x => x.DueAt != null && x.Status != TicketStatus.Resolved && x.Status != TicketStatus.Closed && x.SlaStatus != SlaStatus.Breached).ToListAsync(cancellationToken);
        foreach (var ticket in activeTickets)
        {
            var deadline = ticket.FirstResponseAt is null && ticket.FirstResponseDueAt is not null && ticket.FirstResponseDueAt < ticket.DueAt
                ? ticket.FirstResponseDueAt : ticket.DueAt;
            var target = deadline <= now ? SlaStatus.Breached : deadline <= threshold ? SlaStatus.AtRisk : SlaStatus.OnTrack;
            if (!ticket.UpdateSlaStatus(target)) continue;
            dbContext.TicketHistories.Add(TicketHistory.Create(ticket.Id, null, target == SlaStatus.Breached ? "SlaBreached" : target == SlaStatus.AtRisk ? "SlaAtRisk" : "SlaOnTrack"));
            if (ticket.AssignedAgentId is Guid agentId)
                dbContext.Notifications.Add(Notification.Create(agentId, target == SlaStatus.Breached ? "SlaBreached" : "SlaAtRisk", $"Ticket {ticket.TicketNumber} is {target}.", ticket.Id));
            if (target == SlaStatus.Breached)
            {
                var admins = await dbContext.Users.Where(x => x.Role == UserRole.Admin && x.IsActive).Select(x => x.Id).ToListAsync(cancellationToken);
                dbContext.Notifications.AddRange(admins.Select(adminId => Notification.Create(adminId, "SlaBreached", $"Ticket {ticket.TicketNumber} breached its SLA.", ticket.Id)));
            }
        }
        if (dbContext.ChangeTracker.HasChanges()) await dbContext.SaveChangesAsync(cancellationToken);
    }
}
