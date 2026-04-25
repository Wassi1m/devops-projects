using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class IdeaService : IIdeaService
{
    private readonly AppDbContext _db;

    public IdeaService(AppDbContext db) => _db = db;

    public async Task<IdeaDto> CreerIdeeAsync(CreateIdeaDto dto, int userId)
    {
        var idee = new IdeeInvestissement
        {
            Titre = dto.Titre,
            Description = dto.Description,
            ActionConcernee = dto.ActionConcernee,
            Tendance = dto.Tendance,
            RentabiliteEstimee = dto.RentabiliteEstimee,
            EstPublique = dto.EstPublique,
            TeamId = dto.TeamId,
            ResultatAnalyseId = dto.ResultatAnalyseId,
            UtilisateurId = userId,
            TypeIdee = "Manuelle"
        };

        _db.IdeesInvestissement.Add(idee);
        await _db.SaveChangesAsync();

        var u = await _db.Utilisateurs.FindAsync(userId);
        return MapToDto(idee, u!);
    }

    public async Task<List<IdeaDto>> GetIdeesUtilisateurAsync(int userId)
    {
        return await _db.IdeesInvestissement
            .Include(i => i.Utilisateur)
            .Include(i => i.Team)
            .Where(i => i.UtilisateurId == userId)
            .OrderByDescending(i => i.DateCreation)
            .Select(i => MapToDto(i, i.Utilisateur))
            .ToListAsync();
    }

    public async Task<List<IdeaDto>> GetIdeesTeamAsync(int teamId, int userId)
    {
        var estMembre = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == userId && m.Statut == "Accepté");
        if (!estMembre) return [];

        return await _db.IdeesInvestissement
            .Include(i => i.Utilisateur)
            .Include(i => i.Team)
            .Where(i => i.TeamId == teamId)
            .OrderByDescending(i => i.DateCreation)
            .Select(i => MapToDto(i, i.Utilisateur))
            .ToListAsync();
    }

    public async Task<List<IdeaDto>> GetIdeesPubliquesAsync()
    {
        return await _db.IdeesInvestissement
            .Include(i => i.Utilisateur)
            .Include(i => i.Team)
            .Where(i => i.EstPublique)
            .OrderByDescending(i => i.DateCreation)
            .Select(i => MapToDto(i, i.Utilisateur))
            .ToListAsync();
    }

    public async Task<IdeaDto?> GetIdeeByIdAsync(int id, int userId)
    {
        var idee = await _db.IdeesInvestissement
            .Include(i => i.Utilisateur)
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == id &&
                (i.UtilisateurId == userId || i.EstPublique));
        return idee is null ? null : MapToDto(idee, idee.Utilisateur);
    }

    public async Task<bool> SupprimerIdeeAsync(int id, int userId)
    {
        var idee = await _db.IdeesInvestissement
            .FirstOrDefaultAsync(i => i.Id == id && i.UtilisateurId == userId);
        if (idee is null) return false;

        _db.IdeesInvestissement.Remove(idee);
        await _db.SaveChangesAsync();
        return true;
    }

    private static IdeaDto MapToDto(IdeeInvestissement i, Utilisateur u) => new()
    {
        Id = i.Id,
        Titre = i.Titre,
        Description = i.Description,
        ActionConcernee = i.ActionConcernee,
        TypeIdee = i.TypeIdee,
        Tendance = i.Tendance,
        RentabiliteEstimee = i.RentabiliteEstimee,
        DateCreation = i.DateCreation,
        EstPublique = i.EstPublique,
        UtilisateurId = i.UtilisateurId,
        NomAuteur = $"{u.Prenom} {u.Nom}",
        TeamId = i.TeamId,
        NomTeam = i.Team?.Nom
    };
}
