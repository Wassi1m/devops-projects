using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalysisController : ControllerBase
{
    private readonly IAIAnalysisService _analysisService;

    public AnalysisController(IAIAnalysisService analysisService) => _analysisService = analysisService;

    /// <summary>
    /// Lance l'analyse IA d'une courbe boursière.
    /// L'analyse est effectuée via l'API IA externe si configurée, sinon en mode simulation.
    /// </summary>
    [HttpPost("analyser/{courbeId}")]
    public async Task<IActionResult> Analyser(int courbeId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var result = await _analysisService.AnalyserCourbeAsync(courbeId, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Courbe introuvable ou accès refusé." });
        }
    }

    [HttpGet("resultat/{courbeId}")]
    public async Task<IActionResult> GetResultat(int courbeId)
    {
        var resultat = await _analysisService.GetResultatAsync(courbeId);
        return resultat is null ? NotFound() : Ok(resultat);
    }

    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var historique = await _analysisService.GetHistoriqueAsync(userId);
        return Ok(historique);
    }
}
