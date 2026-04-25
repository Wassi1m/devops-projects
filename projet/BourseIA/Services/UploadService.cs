using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class UploadService : IUploadService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private static readonly string[] ExtensionsAutorisees = [".png", ".jpg", ".jpeg"];

    public UploadService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<CourbeDto> UploadCourbeAsync(IFormFile fichier, string? typeAction, int userId)
    {
        var ext = Path.GetExtension(fichier.FileName).ToLower();
        if (!ExtensionsAutorisees.Contains(ext))
            throw new InvalidOperationException("Seuls les formats PNG, JPG et JPEG sont acceptés.");

        if (fichier.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Le fichier ne doit pas dépasser 10 Mo.");

        var dossier = Path.Combine(_env.WebRootPath, "courbes", userId.ToString());
        Directory.CreateDirectory(dossier);

        var nomFichier = $"{Guid.NewGuid()}{ext}";
        var chemin = Path.Combine(dossier, nomFichier);

        await using var stream = File.Create(chemin);
        await fichier.CopyToAsync(stream);

        var courbe = new CourbeBoursiere
        {
            NomFichier = fichier.FileName,
            CheminFichier = $"/courbes/{userId}/{nomFichier}",
            TypeAction = typeAction,
            UtilisateurId = userId,
            Statut = "EnAttente"
        };

        _db.CourbesBousieres.Add(courbe);
        await _db.SaveChangesAsync();

        return MapToDto(courbe, null);
    }

    public async Task<List<CourbeDto>> GetCourbesUtilisateurAsync(int userId)
    {
        var courbes = await _db.CourbesBousieres
            .Include(c => c.ResultatAnalyse)
            .Include(c => c.Utilisateur)
            .Where(c => c.UtilisateurId == userId)
            .OrderByDescending(c => c.DateUpload)
            .ToListAsync();

        return courbes.Select(c => MapToDto(c, c.ResultatAnalyse)).ToList();
    }

    public async Task<CourbeDto?> GetCourbeByIdAsync(int courbeId, int userId)
    {
        var courbe = await _db.CourbesBousieres
            .Include(c => c.ResultatAnalyse)
            .Include(c => c.Utilisateur)
            .FirstOrDefaultAsync(c => c.Id == courbeId && c.UtilisateurId == userId);

        return courbe is null ? null : MapToDto(courbe, courbe.ResultatAnalyse);
    }

    public async Task<bool> SupprimerCourbeAsync(int courbeId, int userId)
    {
        var courbe = await _db.CourbesBousieres
            .FirstOrDefaultAsync(c => c.Id == courbeId && c.UtilisateurId == userId);

        if (courbe is null) return false;

        var cheminPhysique = Path.Combine(_env.WebRootPath, courbe.CheminFichier.TrimStart('/'));
        if (File.Exists(cheminPhysique))
            File.Delete(cheminPhysique);

        _db.CourbesBousieres.Remove(courbe);
        await _db.SaveChangesAsync();
        return true;
    }

    private static CourbeDto MapToDto(CourbeBoursiere c, ResultatAnalyse? r) => new()
    {
        Id = c.Id,
        NomFichier = c.NomFichier,
        TypeAction = c.TypeAction,
        DateUpload = c.DateUpload,
        Statut = c.Statut,
        UtilisateurId = c.UtilisateurId,
        NomUtilisateur = c.Utilisateur is null ? string.Empty : $"{c.Utilisateur.Prenom} {c.Utilisateur.Nom}",
        Resultat = r is null ? null : new AnalysisResultDto
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
        }
    };
}
