using BourseIA.DTOs;

namespace BourseIA.Services;

public interface ITeamService
{
    Task<TeamDto> CreerTeamAsync(CreateTeamDto dto, int createurId);
    Task<List<TeamDto>> GetTeamsUtilisateurAsync(int userId);
    Task<TeamDetailDto?> GetTeamDetailAsync(int teamId, int userId);
    Task<bool> InviterMembreAsync(InvitationDto dto, int inviteurId);
    Task<bool> RepondreInvitationAsync(RepondreInvitationDto dto, int userId);
    Task<bool> PartagerCourbeAsync(PartagerCourbeDto dto, int userId);
    Task<bool> RetirerPartageAsync(int teamId, int courbeId, int userId);
    Task<bool> QuitterTeamAsync(int teamId, int userId);
    Task<bool> SupprimerTeamAsync(int teamId, int userId);
    Task<bool> KickerMembreAsync(int teamId, int membreId, int adminId);
    Task<bool> BannerMembreAsync(int teamId, int membreId, int adminId);
    Task<bool> DebloquerMembreAsync(int teamId, int membreId, int adminId);
    Task<bool> ChangerRoleAsync(int teamId, int membreId, string nouveauRole, int adminId);
    Task<List<InvitationEnAttenteDto>> GetInvitationsEnAttenteAsync(int userId);
    Task<List<CourbePartageDto>> GetCourbsEquipeAsync(int teamId, int userId);
}
