namespace SmartDesk.Infrastructure.Ai;

public sealed class AiClassificationOptions
{
    public const string SectionName = "AiClassification";
    public decimal ApplyConfidenceThreshold { get; init; } = 0.60m;
    public decimal ReviewConfidenceThreshold { get; init; } = 0.80m;
}
