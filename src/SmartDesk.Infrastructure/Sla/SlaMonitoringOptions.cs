namespace SmartDesk.Infrastructure.Sla;

public sealed class SlaMonitoringOptions
{
    public const string SectionName = "SlaMonitoring";
    public int IntervalMinutes { get; init; } = 10;
    public int AtRiskThresholdMinutes { get; init; } = 60;
}
