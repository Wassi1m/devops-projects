using BourseIA.DTOs;

namespace BourseIA.Services;

/// <summary>
/// Interface pour le service d'analyse IA.
/// L'implémentation réelle sera connectée à un modèle externe via API ultérieurement.
/// </summary>
public interface IAIAnalysisService
{
    Task<AnalysisResultDto> AnalyserCourbeAsync(int courbeId, int userId);
    Task<AnalysisResultDto?> GetResultatAsync(int courbeId);
    Task<List<AnalysisResultDto>> GetHistoriqueAsync(int userId);
}
