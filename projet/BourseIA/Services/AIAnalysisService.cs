using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

/// <summary>
/// Implémentation placeholder du service d'analyse IA.
/// Cette classe simule l'analyse en attendant l'intégration du modèle IA via API externe.
/// Remplacer la méthode AnalyserViaApiExterneAsync() lorsque l'API sera disponible.
/// </summary>
public class AIAnalysisService : IAIAnalysisService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AIAnalysisService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public AIAnalysisService(
        AppDbContext db,
        ILogger<AIAnalysisService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _db = db;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<AnalysisResultDto> AnalyserCourbeAsync(int courbeId, int userId)
    {
        var courbe = await _db.CourbesBousieres
            .FirstOrDefaultAsync(c => c.Id == courbeId && c.UtilisateurId == userId)
            ?? throw new KeyNotFoundException("Courbe introuvable.");

        var existant = await _db.ResultatsAnalyse.FirstOrDefaultAsync(r => r.CourbeBoursiereId == courbeId);
        if (existant is not null && existant.StatutAnalyse == "Terminé")
            return MapToDto(existant);

        var resultat = existant ?? new ResultatAnalyse { CourbeBoursiereId = courbeId };
        resultat.StatutAnalyse = "EnCours";

        if (existant is null) _db.ResultatsAnalyse.Add(resultat);
        courbe.Statut = "EnAnalyse";
        await _db.SaveChangesAsync();

        try
        {
            var apiUrl = _config["AIApi:BaseUrl"];
            if (!string.IsNullOrEmpty(apiUrl))
            {
                await AnalyserViaApiExterneAsync(resultat, courbe, apiUrl);
            }
            else
            {
                SimulerAnalyse(resultat);
                _logger.LogWarning("Mode simulation IA actif. Configurez AIApi:BaseUrl dans appsettings.json pour utiliser l'API externe.");
            }

            resultat.StatutAnalyse = "Terminé";
            courbe.Statut = "Analysé";
        }
        catch (Exception ex)
        {
            resultat.StatutAnalyse = "Erreur";
            resultat.MessageErreur = ex.Message;
            courbe.Statut = "ErreurAnalyse";
            _logger.LogError(ex, "Erreur lors de l'analyse de la courbe {CourbeId}", courbeId);
        }

        await _db.SaveChangesAsync();
        return MapToDto(resultat);
    }

    /// <summary>
    /// Point d'extension pour l'intégration de l'API IA externe.
    /// Cette méthode sera complétée lorsque le modèle IA sera disponible.
    /// </summary>
    private async Task AnalyserViaApiExterneAsync(ResultatAnalyse resultat, CourbeBoursiere courbe, string apiUrl)
    {
        var client = _httpClientFactory.CreateClient("AIClient");

        using var form = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead(Path.Combine("wwwroot", courbe.CheminFichier.TrimStart('/')));
        form.Add(new StreamContent(fileStream), "image", courbe.NomFichier);

        var response = await client.PostAsync($"{apiUrl}/analyze", form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        resultat.RapportJson = json;

        var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        if (data is not null)
        {
            resultat.Tendance = data.TryGetValue("tendance", out var t) ? t.ToString() ?? "Inconnue" : "Inconnue";
            resultat.PointsCles = data.TryGetValue("points_cles", out var p) ? p.ToString() : null;

            if (data.TryGetValue("prix_min", out var pmin) && double.TryParse(pmin.ToString(), out var dmin))
                resultat.PrixMin = dmin;
            if (data.TryGetValue("prix_max", out var pmax) && double.TryParse(pmax.ToString(), out var dmax))
                resultat.PrixMax = dmax;
            if (data.TryGetValue("prix_moyen", out var pmoy) && double.TryParse(pmoy.ToString(), out var dmoy))
                resultat.PrixMoyen = dmoy;
            if (data.TryGetValue("ecart_type", out var et) && double.TryParse(et.ToString(), out var det))
                resultat.EcartType = det;
        }
    }

    private static void SimulerAnalyse(ResultatAnalyse resultat)
    {
        var rng = new Random();
        var tendances = new[] { "Hausse", "Baisse", "Stable" };

        resultat.Tendance = tendances[rng.Next(tendances.Length)];
        resultat.PrixMin = Math.Round(rng.NextDouble() * 50 + 10, 2);
        resultat.PrixMax = Math.Round(resultat.PrixMin.Value + rng.NextDouble() * 100, 2);
        resultat.PrixMoyen = Math.Round((resultat.PrixMin.Value + resultat.PrixMax.Value) / 2, 2);
        resultat.EcartType = Math.Round(rng.NextDouble() * 15, 2);
        resultat.PointsCles = $"Support: {resultat.PrixMin}, Résistance: {resultat.PrixMax}";
        resultat.RapportJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            mode = "simulation",
            tendance = resultat.Tendance,
            prix_min = resultat.PrixMin,
            prix_max = resultat.PrixMax,
            prix_moyen = resultat.PrixMoyen,
            ecart_type = resultat.EcartType
        });
    }

    public async Task<AnalysisResultDto?> GetResultatAsync(int courbeId)
    {
        var r = await _db.ResultatsAnalyse.FirstOrDefaultAsync(r => r.CourbeBoursiereId == courbeId);
        return r is null ? null : MapToDto(r);
    }

    public async Task<List<AnalysisResultDto>> GetHistoriqueAsync(int userId)
    {
        return await _db.ResultatsAnalyse
            .Include(r => r.CourbeBoursiere)
            .Where(r => r.CourbeBoursiere.UtilisateurId == userId)
            .OrderByDescending(r => r.DateAnalyse)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    private static AnalysisResultDto MapToDto(ResultatAnalyse r) => new()
    {
        Id = r.Id,
        Tendance = r.Tendance,
        PrixMin = r.PrixMin,
        PrixMax = r.PrixMax,
        PrixMoyen = r.PrixMoyen,
        EcartType = r.EcartType,
        PointsCles = r.PointsCles,
        RapportJson = r.RapportJson,
        StatutAnalyse = r.StatutAnalyse,
        DateAnalyse = r.DateAnalyse,
        MessageErreur = r.MessageErreur,
        CourbeBoursiereId = r.CourbeBoursiereId
    };
}
