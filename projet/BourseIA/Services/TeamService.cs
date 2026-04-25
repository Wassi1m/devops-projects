using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class TeamService : ITeamService
{
    private readonly AppDbContext _db;

    public TeamService(AppDbContext db) => _db = db;

    public async Task<TeamDto> CreerTeamAsync(CreateTeamDto dto, int createurId)
    {
        var team = new Team
        {
            Nom = dto.Nom,
            Description = dto.Description,
            CreateurId = createurId
        };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync();

        _db.MembresEquipe.Add(new MembreEquipe
        {
            UtilisateurId = createurId,
            TeamId = team.Id,
            Role = "Admin",
            Statut = "Accepté"
        });
        await _db.SaveChangesAsync();

        var createur = await _db.Utilisateurs.FindAsync(createurId);
        return MapToDto(team, createur!, 1, "Admin");
    }

    public async Task<List<TeamDto>> GetTeamsUtilisateurAsync(int userId)
    {
        var memberships = await _db.MembresEquipe
            .Include(m => m.Team).ThenInclude(t => t.Createur)
            .Include(m => m.Team).ThenInclude(t => t.Membres)
            .Where(m => m.UtilisateurId == userId && m.Statut == "Accepté")
            .ToListAsync();

        return memberships.Select(m => MapToDto(
            m.Team, m.Team.Createur,
            m.Team.Membres.Count(x => x.Statut == "Accepté"),
            m.Role)).ToList();
    }

    public async Task<TeamDetailDto?> GetTeamDetailAsync(int teamId, int userId)
    {
        var estMembre = await _db.MembresEquipe
            .AnyAsync(m => m.TeamId == teamId && m.UtilisateurId == userId && m.Statut == "Accepté");
        if (!estMembre) return null;

        var team = await _db.Teams
            .Include(t => t.Createur)
            .Include(t => t.Membres).ThenInclude(m => m.Utilisateur)
            .Include(t => t.Partages).ThenInclude(p => p.CourbeBoursiere).ThenInclude(c => c.ResultatAnalyse)
            .Include(t => t.Partages).ThenInclude(p => p.CourbeBoursiere).ThenInclude(c => c.Utilisateur)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null) return null;

        var monRole = team.Membres.First(m => m.UtilisateurId == userId).Role;

        return new TeamDetailDto
        {
            Id = team.Id,
            Nom = team.Nom,
            Description = team.Description,
            DateCreation = team.DateCreation,
            CreateurId = team.CreateurId,
            NomCreateur = $"{team.Createur.Prenom} {team.Createur.Nom}",
            NombreMembres = team.Membres.Count(m => m.Statut == "Accepté"),
            MonRole = monRole,
            Membres = team.Membres.Select(m => new MembreEquipeDto
            {
                UtilisateurId = m.UtilisateurId,
                Nom = m.Utilisateur.Nom,
                Prenom = m.Utilisateur.Prenom,
                Email = m.Utilisateur.Email,
                Role = m.Role,
                Statut = m.Statut,
                DateAdhesion = m.DateAdhesion
            }).ToList(),
            Courbes = team.Partages.Select(p => new CourbeDto
            {
                Id = p.CourbeBoursiere.Id,
                NomFichier = p.CourbeBoursiere.NomFichier,
                TypeAction = p.CourbeBoursiere.TypeAction,
                DateUpload = p.CourbeBoursiere.DateUpload,
                Statut = p.CourbeBoursiere.Statut,
                UtilisateurId = p.CourbeBoursiere.UtilisateurId,
                NomUtilisateur = $"{p.CourbeBoursiere.Utilisateur.Prenom} {p.CourbeBoursiere.Utilisateur.Nom}"
            }).ToList()
        };
    }

    public async Task<bool> InviterMembreAsync(InvitationDto dto, int inviteurId)
    {
        var estAdmin = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == dto.TeamId && m.UtilisateurId == inviteurId &&
            m.Role == "Admin" && m.Statut == "Accepté");
        if (!estAdmin) return false;

        int? inviteId = dto.UtilisateurId;
        if (inviteId is null && dto.EmailInvite is not null)
        {
            var u = await _db.Utilisateurs.FirstOrDefaultAsync(u => u.Email == dto.EmailInvite);
            inviteId = u?.Id;
        }
        if (inviteId is null) return false;

        // Ne pas inviter l'admin lui-même
        if (inviteId == inviteurId) return false;

        var existant = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == dto.TeamId && m.UtilisateurId == inviteId);

        if (existant is not null)
        {
            // Déjà membre actif → bloquer
            if (existant.Statut == "Accepté") return false;
            // Banni → seul un déblocage explicite peut réintégrer
            if (existant.Statut == "Banni") return false;
            // Invitation déjà en attente → réinitialiser la date et renvoyer
            existant.DateAdhesion = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        _db.MembresEquipe.Add(new MembreEquipe
        {
            TeamId = dto.TeamId,
            UtilisateurId = inviteId.Value,
            Role = "Membre",
            Statut = "EnAttente"
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RepondreInvitationAsync(RepondreInvitationDto dto, int userId)
    {
        var invitation = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == dto.TeamId && m.UtilisateurId == userId && m.Statut == "EnAttente");
        if (invitation is null) return false;

        if (dto.Accepter)
            invitation.Statut = "Accepté";
        else
            _db.MembresEquipe.Remove(invitation);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PartagerCourbeAsync(PartagerCourbeDto dto, int userId)
    {
        var estMembre = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == dto.TeamId && m.UtilisateurId == userId && m.Statut == "Accepté");
        if (!estMembre) return false;

        var courbe = await _db.CourbesBousieres.FirstOrDefaultAsync(c =>
            c.Id == dto.CourbeId && c.UtilisateurId == userId);
        if (courbe is null) return false;

        var dejaPartagee = await _db.PartagesEquipe.AnyAsync(p =>
            p.TeamId == dto.TeamId && p.CourbeBoursiereId == dto.CourbeId);
        if (dejaPartagee) return false;

        _db.PartagesEquipe.Add(new PartageEquipe
        {
            TeamId = dto.TeamId,
            CourbeBoursiereId = dto.CourbeId,
            PartageParId = userId,
            Commentaire = dto.Commentaire
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> QuitterTeamAsync(int teamId, int userId)
    {
        var membre = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == userId);
        if (membre is null) return false;

        _db.MembresEquipe.Remove(membre);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SupprimerTeamAsync(int teamId, int userId)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CreateurId == userId);
        if (team is null) return false;

        _db.Teams.Remove(team);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> KickerMembreAsync(int teamId, int membreId, int adminId)
    {
        var admin = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == adminId && m.Role == "Admin" && m.Statut == "Accepté");
        if (admin is null) return false;

        if (membreId == adminId) return false;

        var membre = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == membreId);
        if (membre is null) return false;

        _db.MembresEquipe.Remove(membre);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BannerMembreAsync(int teamId, int membreId, int adminId)
    {
        var admin = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == adminId && m.Role == "Admin" && m.Statut == "Accepté");
        if (admin is null) return false;

        if (membreId == adminId) return false;

        var membre = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == membreId);
        if (membre is null) return false;

        membre.Statut = "Banni";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DebloquerMembreAsync(int teamId, int membreId, int adminId)
    {
        var admin = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == adminId && m.Role == "Admin" && m.Statut == "Accepté");
        if (admin is null) return false;

        var membre = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == membreId && m.Statut == "Banni");
        if (membre is null) return false;

        membre.Statut = "Accepté";
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangerRoleAsync(int teamId, int membreId, string nouveauRole, int adminId)
    {
        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == teamId && t.CreateurId == adminId);
        if (team is null) return false;

        if (membreId == adminId) return false;

        var membre = await _db.MembresEquipe.FirstOrDefaultAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == membreId && m.Statut == "Accepté");
        if (membre is null) return false;

        membre.Role = nouveauRole;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<InvitationEnAttenteDto>> GetInvitationsEnAttenteAsync(int userId)
    {
        var invitations = await _db.MembresEquipe
            .Include(m => m.Team).ThenInclude(t => t.Createur)
            .Where(m => m.UtilisateurId == userId && m.Statut == "EnAttente")
            .ToListAsync();

        return invitations.Select(m => new InvitationEnAttenteDto
        {
            TeamId = m.TeamId,
            TeamNom = m.Team.Nom,
            CreateurNom = $"{m.Team.Createur.Prenom} {m.Team.Createur.Nom}",
            DateInvitation = m.DateAdhesion
        }).ToList();
    }

    public async Task<List<CourbePartageDto>> GetCourbsEquipeAsync(int teamId, int userId)
    {
        var estMembre = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == userId && m.Statut == "Accepté");
        if (!estMembre) return [];

        var partages = await _db.PartagesEquipe
            .Include(p => p.CourbeBoursiere).ThenInclude(c => c.ResultatAnalyse)
            .Include(p => p.CourbeBoursiere).ThenInclude(c => c.Utilisateur)
            .Include(p => p.PartagePar)
            .Where(p => p.TeamId == teamId)
            .OrderByDescending(p => p.DatePartage)
            .ToListAsync();

        return partages.Select(p => new CourbePartageDto
        {
            PartageId = p.Id,
            CourbeId = p.CourbeBoursiereId,
            NomFichier = p.CourbeBoursiere.NomFichier,
            TypeAction = p.CourbeBoursiere.TypeAction,
            DateUpload = p.CourbeBoursiere.DateUpload,
            Statut = p.CourbeBoursiere.Statut,
            PartageParId = p.PartageParId,
            NomPartage = $"{p.PartagePar.Prenom} {p.PartagePar.Nom}",
            Commentaire = p.Commentaire,
            DatePartage = p.DatePartage,
            Analyse = p.CourbeBoursiere.ResultatAnalyse == null ? null : new AnalysisResultDto
            {
                Id = p.CourbeBoursiere.ResultatAnalyse.Id,
                Tendance = p.CourbeBoursiere.ResultatAnalyse.Tendance,
                PrixMin = p.CourbeBoursiere.ResultatAnalyse.PrixMin,
                PrixMax = p.CourbeBoursiere.ResultatAnalyse.PrixMax,
                PrixMoyen = p.CourbeBoursiere.ResultatAnalyse.PrixMoyen,
                EcartType = p.CourbeBoursiere.ResultatAnalyse.EcartType,
                StatutAnalyse = p.CourbeBoursiere.ResultatAnalyse.StatutAnalyse,
                DateAnalyse = p.CourbeBoursiere.ResultatAnalyse.DateAnalyse,
                CourbeBoursiereId = p.CourbeBoursiere.Id
            }
        }).ToList();
    }

    public async Task<bool> RetirerPartageAsync(int teamId, int courbeId, int userId)
    {
        var partage = await _db.PartagesEquipe.FirstOrDefaultAsync(p =>
            p.TeamId == teamId && p.CourbeBoursiereId == courbeId);
        if (partage is null) return false;

        var estAdmin = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == userId && m.Role == "Admin" && m.Statut == "Accepté");
        var estCreateur = partage.PartageParId == userId;
        if (!estAdmin && !estCreateur) return false;

        _db.PartagesEquipe.Remove(partage);
        await _db.SaveChangesAsync();
        return true;
    }

    private static TeamDto MapToDto(Team t, Utilisateur createur, int nbMembres, string monRole) => new()
    {
        Id = t.Id,
        Nom = t.Nom,
        Description = t.Description,
        DateCreation = t.DateCreation,
        CreateurId = t.CreateurId,
        NomCreateur = $"{createur.Prenom} {createur.Nom}",
        NombreMembres = nbMembres,
        MonRole = monRole
    };
}
