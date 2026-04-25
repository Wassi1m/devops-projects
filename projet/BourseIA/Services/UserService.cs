using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using BourseIA.Utils;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwt;
    private readonly IWebHostEnvironment _env;

    public UserService(AppDbContext db, JwtHelper jwt, IWebHostEnvironment env)
    {
        _db = db;
        _jwt = jwt;
        _env = env;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _db.Utilisateurs.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Un compte avec cet email existe déjà.");

        var utilisateur = new Utilisateur
        {
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Email = dto.Email,
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(dto.MotDePasse),
            ProfilInvestisseur = dto.ProfilInvestisseur
        };

        _db.Utilisateurs.Add(utilisateur);
        await _db.SaveChangesAsync();

        var (token, expiration) = _jwt.GenererToken(utilisateur);
        return new AuthResponseDto
        {
            Token = token,
            Expiration = expiration,
            Utilisateur = MapToDto(utilisateur)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var utilisateur = await _db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

        if (!BCrypt.Net.BCrypt.Verify(dto.MotDePasse, utilisateur.MotDePasseHash))
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");

        var (token, expiration) = _jwt.GenererToken(utilisateur);
        return new AuthResponseDto
        {
            Token = token,
            Expiration = expiration,
            Utilisateur = MapToDto(utilisateur)
        };
    }

    public async Task<UtilisateurDto?> GetByIdAsync(int id)
    {
        var u = await _db.Utilisateurs.FindAsync(id);
        return u is null ? null : MapToDto(u);
    }

    public async Task<UtilisateurDto> UpdateProfilAsync(int id, UpdateProfilDto dto)
    {
        var u = await _db.Utilisateurs.FindAsync(id)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        if (dto.Nom is not null) u.Nom = dto.Nom;
        if (dto.Prenom is not null) u.Prenom = dto.Prenom;
        if (dto.ProfilInvestisseur is not null) u.ProfilInvestisseur = dto.ProfilInvestisseur;

        await _db.SaveChangesAsync();
        return MapToDto(u);
    }

    public async Task<string?> UpdatePhotoProfilAsync(int id, IFormFile photo)
    {
        var u = await _db.Utilisateurs.FindAsync(id)
            ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        var dossier = Path.Combine(_env.WebRootPath, "profils");
        Directory.CreateDirectory(dossier);

        var nomFichier = $"{id}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var chemin = Path.Combine(dossier, nomFichier);

        await using var stream = File.Create(chemin);
        await photo.CopyToAsync(stream);

        u.PhotoProfil = $"/profils/{nomFichier}";
        await _db.SaveChangesAsync();
        return u.PhotoProfil;
    }

    public async Task<StatistiquesDto> GetStatistiquesAsync(int userId)
    {
        var analyses = await _db.ResultatsAnalyse
            .Include(r => r.CourbeBoursiere)
            .Where(r => r.CourbeBoursiere.UtilisateurId == userId && r.StatutAnalyse == "Terminé")
            .OrderByDescending(r => r.DateAnalyse)
            .ToListAsync();

        return new StatistiquesDto
        {
            TotalAnalyses = analyses.Count,
            AnalysesEnHausse = analyses.Count(a => a.Tendance == "Hausse"),
            AnalysesEnBaisse = analyses.Count(a => a.Tendance == "Baisse"),
            AnalysesStables = analyses.Count(a => a.Tendance == "Stable"),
            PrixMoyenGlobal = analyses.Any(a => a.PrixMoyen.HasValue)
                ? analyses.Where(a => a.PrixMoyen.HasValue).Average(a => a.PrixMoyen!.Value)
                : null,
            DernieresAnalyses = analyses.Take(5).Select(MapAnalyse).ToList()
        };
    }

    private static UtilisateurDto MapToDto(Utilisateur u) => new()
    {
        Id = u.Id,
        Nom = u.Nom,
        Prenom = u.Prenom,
        Email = u.Email,
        ProfilInvestisseur = u.ProfilInvestisseur,
        PhotoProfil = u.PhotoProfil,
        DateInscription = u.DateInscription
    };

    private static AnalysisResultDto MapAnalyse(ResultatAnalyse r) => new()
    {
        Id = r.Id,
        Tendance = r.Tendance,
        PrixMin = r.PrixMin,
        PrixMax = r.PrixMax,
        PrixMoyen = r.PrixMoyen,
        EcartType = r.EcartType,
        PointsCles = r.PointsCles,
        StatutAnalyse = r.StatutAnalyse,
        DateAnalyse = r.DateAnalyse,
        CourbeBoursiereId = r.CourbeBoursiereId
    };
}
