using Microsoft.ML;
using Microsoft.ML.Data;
using SmartDesk.Application.Ai;
using SmartDesk.Domain.Enums;

namespace SmartDesk.Infrastructure.Ai;

public sealed class MlNetTicketClassificationService : ITicketClassificationService
{
    private readonly MLContext _ml = new(seed: 42);
    private readonly ITransformer _categoryModel;
    private readonly ITransformer _priorityModel;
    private readonly object _sync = new();

    public MlNetTicketClassificationService()
    {
        var data = _ml.Data.LoadFromEnumerable(SyntheticTrainingData.Create());
        _categoryModel = Train(data, nameof(TrainingExample.Category));
        _priorityModel = Train(data, nameof(TrainingExample.Priority));
    }

    public Task<TicketClassification> ClassifyAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var input = new TrainingExample { Text = $"{title} {description}" };
        lock (_sync)
        {
            var category = _ml.Model.CreatePredictionEngine<TrainingExample, ClassificationPrediction>(_categoryModel).Predict(input);
            var priority = _ml.Model.CreatePredictionEngine<TrainingExample, ClassificationPrediction>(_priorityModel).Predict(input);
            var confidence = Math.Round((decimal)(MaxProbability(category.Score) + MaxProbability(priority.Score)) / 2m, 4);
            return Task.FromResult(new TicketClassification(category.Label, Enum.Parse<TicketPriority>(priority.Label), confidence));
        }
    }

    private ITransformer Train(IDataView data, string labelColumn) => _ml.Transforms.Conversion.MapValueToKey("Label", labelColumn)
        .Append(_ml.Transforms.Text.FeaturizeText("Features", nameof(TrainingExample.Text)))
        .Append(_ml.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
        .Append(_ml.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
        .Fit(data);

    private static float MaxProbability(float[] scores)
    {
        if (scores.Length == 0) return 0f;
        return scores.Max();
    }

    private sealed class ClassificationPrediction { [ColumnName("PredictedLabel")] public string Label { get; set; } = string.Empty; public float[] Score { get; set; } = []; }
}

internal sealed class TrainingExample { public string Text { get; set; } = string.Empty; public string Category { get; set; } = string.Empty; public string Priority { get; set; } = string.Empty; }

internal static class SyntheticTrainingData
{
    public static IEnumerable<TrainingExample> Create()
    {
        var phrases = new Dictionary<string, string[]> {
            ["Network"] = ["wifi cannot connect", "vpn keeps disconnecting", "network connection is unavailable", "internet access is slow"],
            ["Hardware"] = ["laptop keyboard is not working", "monitor will not power on", "mouse has stopped responding", "battery does not charge"],
            ["Software"] = ["application crashes on startup", "program installation fails", "desktop software shows an error", "browser is freezing"],
            ["Account Access"] = ["forgot my corporate password", "account is locked out", "cannot sign in to the portal", "multi factor authentication fails"],
            ["Security"] = ["suspicious phishing email received", "malware warning appeared", "device may be compromised", "security certificate error"],
            ["Email"] = ["outlook crashes opening attachments", "email messages are not sending", "mailbox is full", "calendar invite is missing"],
            ["Infrastructure"] = ["production server is unavailable", "database service is down", "deployment pipeline failed", "shared storage is offline"] };
        var urgency = new Dictionary<TicketPriority, string[]> {
            [TicketPriority.Low] = ["when convenient", "no immediate impact", "minor issue", "can wait"],
            [TicketPriority.Medium] = ["affecting my daily work", "need help today", "normal business impact", "please investigate"],
            [TicketPriority.High] = ["multiple users are blocked", "urgent business impact", "team cannot work", "needs attention quickly"],
            [TicketPriority.Critical] = ["production outage", "all users are blocked", "critical service is down", "major incident now"] };
        for (var variation = 0; variation < 12; variation++)
            foreach (var (category, categoryPhrases) in phrases)
                foreach (var (priority, urgencyPhrases) in urgency)
                {
                    var phrase = categoryPhrases[variation % categoryPhrases.Length];
                    var urgencyPhrase = urgencyPhrases[(variation / 2) % urgencyPhrases.Length];
                    yield return new TrainingExample { Text = $"{phrase}. {urgencyPhrase}. Report variant {variation}.", Category = category, Priority = priority.ToString() };
                }
    }
}
