using BourseIA.DTOs;
using System.Text.Json;

namespace BourseIA.Services;

/// <summary>
/// Interface pour le service d'analyse de confiance
/// </summary>
public interface IConfidenceAnalysisService
{
    /// <summary>
    /// Analyse la confiance des prédictions
    /// </summary>
    ConfidenceAnalysisDto AnalyzePredictions(PredictionResponseDto response, decimal threshold = 0.7m);

    /// <summary>
    /// Filtre les prédictions par seuil de confiance (above=true → ≥ seuil, false → &lt; seuil)
    /// </summary>
    List<PredictionDto> FilterByConfidence(List<PredictionDto> predictions, decimal threshold, bool above = true);

    /// <summary>
    /// Calcule le score de confiance global
    /// </summary>
    decimal CalculateGlobalConfidence(List<PredictionDto> predictions);

    /// <summary>
    /// Génère un rapport d'analyse de confiance
    /// </summary>
    string GenerateConfidenceReport(ConfidenceAnalysisDto analysis);
}

/// <summary>
/// Service d'analyse de confiance pour les prédictions
/// </summary>
public class ConfidenceAnalysisService : IConfidenceAnalysisService
{
    private readonly ILogger<ConfidenceAnalysisService> _logger;

    public ConfidenceAnalysisService(ILogger<ConfidenceAnalysisService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyse la confiance des prédictions
    /// </summary>
    public ConfidenceAnalysisDto AnalyzePredictions(PredictionResponseDto response, decimal threshold = 0.7m)
    {
        if (response?.Predictions == null || response.Predictions.Count == 0)
        {
            _logger.LogWarning("Aucune prédiction trouvée pour l'analyse de confiance");
            return new ConfidenceAnalysisDto
            {
                PredictionsCount = 0,
                AverageConfidence = 0,
                MaxConfidence = 0,
                MinConfidence = 0,
                ConfidenceThreshold = threshold
            };
        }

        var analysis = new ConfidenceAnalysisDto
        {
            ConfidenceThreshold = threshold,
            PredictionsCount = response.Predictions.Count,
            AverageConfidence = response.Predictions.Average(p => p.Confidence),
            MaxConfidence = response.Predictions.Max(p => p.Confidence),
            MinConfidence = response.Predictions.Min(p => p.Confidence),
            HighConfidencePredictions = FilterByConfidence(response.Predictions, threshold),
            LowConfidencePredictions = FilterByConfidence(response.Predictions, threshold, false)
        };

        _logger.LogInformation(
            "Analyse de confiance complétée: Moyenne={AvgConfidence:P2}, Max={MaxConfidence:P2}, Min={MinConfidence:P2}",
            analysis.AverageConfidence,
            analysis.MaxConfidence,
            analysis.MinConfidence
        );

        return analysis;
    }

    /// <summary>
    /// Filtre les prédictions par seuil de confiance
    /// </summary>
    public List<PredictionDto> FilterByConfidence(List<PredictionDto> predictions, decimal threshold, bool above = true)
    {
        if (predictions == null || predictions.Count == 0)
            return new List<PredictionDto>();

        return above
            ? predictions.Where(p => p.Confidence >= threshold).ToList()
            : predictions.Where(p => p.Confidence < threshold).ToList();
    }

    /// <summary>
    /// Calcule le score de confiance global
    /// </summary>
    public decimal CalculateGlobalConfidence(List<PredictionDto> predictions)
    {
        if (predictions == null || predictions.Count == 0)
            return 0;

        // Calcul pondéré : moyenne + médiane + écart-type
        var average = predictions.Average(p => p.Confidence);
        var sorted = predictions.OrderBy(p => p.Confidence).ToList();
        var median = sorted.Count % 2 == 0
            ? (sorted[sorted.Count / 2 - 1].Confidence + sorted[sorted.Count / 2].Confidence) / 2
            : sorted[sorted.Count / 2].Confidence;

        var variance = predictions.Average(p => Math.Pow((double)(p.Confidence - average), 2));
        var stdDev = Math.Sqrt(variance);

        // Score global = 60% moyenne + 40% stabilité (inverse de stdDev)
        var globalScore = (average * 0.6m) + ((1 - (decimal)stdDev) * 0.4m);
        return Math.Min(globalScore, 1m); // Plafonner à 1.0
    }

    /// <summary>
    /// Génère un rapport d'analyse de confiance
    /// </summary>
    public string GenerateConfidenceReport(ConfidenceAnalysisDto analysis)
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine("╔════════════════════════════════════════════════════╗");
        report.AppendLine("║      RAPPORT D'ANALYSE DE CONFIANCE              ║");
        report.AppendLine("╚════════════════════════════════════════════════════╝");
        report.AppendLine();

        report.AppendLine("📊 STATISTIQUES GLOBALES");
        report.AppendLine($"  • Nombre de prédictions : {analysis.PredictionsCount}");
        report.AppendLine($"  • Confiance moyenne    : {analysis.AverageConfidence:P2}");
        report.AppendLine($"  • Confiance max        : {analysis.MaxConfidence:P2}");
        report.AppendLine($"  • Confiance min        : {analysis.MinConfidence:P2}");
        report.AppendLine();

        report.AppendLine("✅ PRÉDICTIONS DE HAUTE CONFIANCE (≥ {analysis.ConfidenceThreshold:P0})");
        if (analysis.HighConfidencePredictions.Count > 0)
        {
            foreach (var pred in analysis.HighConfidencePredictions.OrderByDescending(p => p.Confidence))
            {
                report.AppendLine($"  ✓ {pred.Label}: {pred.Confidence:P2}");
                if (!string.IsNullOrEmpty(pred.Signal))
                    report.AppendLine($"    Signal: {pred.Signal}");
            }
        }
        else
        {
            report.AppendLine("  ⚠️  Aucune prédiction de haute confiance");
        }
        report.AppendLine();

        report.AppendLine($"⚠️  PRÉDICTIONS DE FAIBLE CONFIANCE (< {analysis.ConfidenceThreshold:P0})");
        if (analysis.LowConfidencePredictions.Count > 0)
        {
            foreach (var pred in analysis.LowConfidencePredictions.OrderBy(p => p.Confidence))
            {
                report.AppendLine($"  ✗ {pred.Label}: {pred.Confidence:P2}");
            }
        }
        else
        {
            report.AppendLine("  ✓ Aucune prédiction de faible confiance");
        }
        report.AppendLine();

        report.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        return report.ToString();
    }
}

