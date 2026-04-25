using Microsoft.AspNetCore.Mvc;
using BourseIA.DTOs;
using BourseIA.Services;
using System.Text.Json;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PredictController> _logger;
    private readonly IConfidenceAnalysisService _confidenceService;

    public PredictController(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<PredictController> logger,
        IConfidenceAnalysisService confidenceService)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
        _confidenceService = confidenceService;
    }

    /// <summary>
    /// Détecte les actions boursières à partir d'une image (réponse brute)
    /// </summary>
    /// <param name="file">Fichier image à analyser</param>
    /// <returns>Réponse brute de l'API de détection</returns>
    [HttpPost]
    [Route("raw")]
    public async Task<IActionResult> PostRaw(IFormFile file)
    {
        return await SendPredictionRequest(file, analyzeConfidence: false);
    }

    /// <summary>
    /// Détecte les actions boursières avec analyse de confiance
    /// </summary>
    /// <param name="file">Fichier image à analyser</param>
    /// <param name="confidenceThreshold">Seuil de confiance (0-1, défaut: 0.7)</param>
    /// <returns>Réponse avec analyse de confiance</returns>
    [HttpPost]
    [Route("analyze")]
    public async Task<IActionResult> PostWithAnalysis([FromForm] IFormFile file, [FromQuery] decimal confidenceThreshold = 0.7m)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                detail = new[]
                {
                    new
                    {
                        type = "missing",
                        loc = new[] { "body", "file" },
                        msg = "Field required",
                        input = (object?)null
                    }
                }
            });
        }

        if (confidenceThreshold < 0 || confidenceThreshold > 1)
        {
            return BadRequest(new { error = "Confidence threshold doit être entre 0 et 1" });
        }

        try
        {
            var responseContent = await GetPredictionResponse(file);

            // Parser la réponse JSON
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<PredictionResponseDto>(responseContent, options);

            if (response == null)
            {
                _logger.LogWarning("Impossible de parser la réponse de prédiction");
                return StatusCode(500, new { error = "Format de réponse invalide" });
            }

            // Analyser la confiance
            var analysis = _confidenceService.AnalyzePredictions(response, confidenceThreshold);
            var report = _confidenceService.GenerateConfidenceReport(analysis);

            _logger.LogInformation("Analyse de confiance réussie pour {FileName}", file.FileName);

            return Ok(new
            {
                success = true,
                prediction = response,
                confidence_analysis = analysis,
                report = report,
                timestamp = DateTime.UtcNow
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion à l'API de détection");
            return StatusCode(503, new { error = "Service de détection indisponible", detail = ex.Message });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erreur lors du parsing de la réponse JSON");
            return StatusCode(500, new { error = "Erreur lors du parsing de la réponse", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la prédiction");
            return StatusCode(500, new { error = "Erreur serveur", detail = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint par défaut (pour compatibilité)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Post(IFormFile file)
    {
        return await SendPredictionRequest(file, analyzeConfidence: true);
    }

    /// <summary>
    /// Envoie la requête de prédiction à l'API de détection
    /// </summary>
    private async Task<IActionResult> SendPredictionRequest(IFormFile file, bool analyzeConfidence = true)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                detail = new[]
                {
                    new
                    {
                        type = "missing",
                        loc = new[] { "body", "file" },
                        msg = "Field required",
                        input = (object?)null
                    }
                }
            });
        }

        try
        {
            var responseContent = await GetPredictionResponse(file);

            if (!analyzeConfidence)
            {
                var contentType = "application/json";
                _logger.LogInformation("Prédiction brute retournée pour {FileName}", file.FileName);
                return Content(responseContent, contentType, System.Text.Encoding.UTF8);
            }

            // Analyser la confiance avec seuil par défaut
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = JsonSerializer.Deserialize<PredictionResponseDto>(responseContent, options);

            if (response == null)
            {
                return Content(responseContent, "application/json", System.Text.Encoding.UTF8);
            }

            var analysis = _confidenceService.AnalyzePredictions(response, 0.7m);

            return Ok(new
            {
                success = true,
                prediction = response,
                confidence_analysis = analysis,
                timestamp = DateTime.UtcNow
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion à l'API de détection");
            return StatusCode(503, new { error = "Service de détection indisponible", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la prédiction");
            return StatusCode(500, new { error = "Erreur serveur", detail = ex.Message });
        }
    }

    /// <summary>
    /// Récupère la réponse brute de l'API de détection
    /// </summary>
    private async Task<string> GetPredictionResponse(IFormFile file)
    {
        var detectionUrl = _config["DetectionApi:Url"] ?? "http://host.docker.internal:8000/predict";
        var client = _httpFactory.CreateClient();

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            file.ContentType ?? "application/octet-stream"
        );
        content.Add(fileContent, "file", file.FileName);

        _logger.LogInformation("Envoi du fichier {FileName} à l'API de détection: {Url}", file.FileName, detectionUrl);

        var response = await client.PostAsync(detectionUrl, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Réponse reçue: {StatusCode}", response.StatusCode);

        return responseBody;
    }
}

