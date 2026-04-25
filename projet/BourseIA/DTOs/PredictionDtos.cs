using System.Text.Json.Serialization;

namespace BourseIA.DTOs;

/// <summary>
/// DTO pour une prédiction d'action boursière
/// </summary>
public class PredictionDto
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }

    [JsonPropertyName("signal")]
    public string? Signal { get; set; }

    [JsonPropertyName("probabilities")]
    public ProbabilitiesDto? Probabilities { get; set; }
}

/// <summary>
/// DTO pour les probabilités de tendance
/// </summary>
public class ProbabilitiesDto
{
    [JsonPropertyName("UP")]
    public decimal Up { get; set; }

    [JsonPropertyName("DOWN")]
    public decimal Down { get; set; }

    [JsonPropertyName("HOLD")]
    public decimal Hold { get; set; }
}

/// <summary>
/// DTO pour la réponse complète de prédiction
/// </summary>
public class PredictionResponseDto
{
    [JsonPropertyName("predictions")]
    public List<PredictionDto> Predictions { get; set; } = new();

    [JsonPropertyName("confidence_score")]
    public decimal? ConfidenceScore { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("signal")]
    public string? Signal { get; set; }

    [JsonPropertyName("probabilities")]
    public ProbabilitiesDto? Probabilities { get; set; }
}

/// <summary>
/// DTO pour l'analyse de confiance
/// </summary>
public class ConfidenceAnalysisDto
{
    [JsonPropertyName("average_confidence")]
    public decimal AverageConfidence { get; set; }

    [JsonPropertyName("max_confidence")]
    public decimal MaxConfidence { get; set; }

    [JsonPropertyName("min_confidence")]
    public decimal MinConfidence { get; set; }

    [JsonPropertyName("predictions_count")]
    public int PredictionsCount { get; set; }

    [JsonPropertyName("high_confidence_predictions")]
    public List<PredictionDto> HighConfidencePredictions { get; set; } = new();

    [JsonPropertyName("low_confidence_predictions")]
    public List<PredictionDto> LowConfidencePredictions { get; set; } = new();

    [JsonPropertyName("confidence_threshold")]
    public decimal ConfidenceThreshold { get; set; } = 0.7m;
}

