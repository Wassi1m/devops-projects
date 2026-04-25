using BourseIA.DTOs;

namespace BourseIA.Services;

public interface IIdeaService
{
    Task<IdeaDto> CreerIdeeAsync(CreateIdeaDto dto, int userId);
    Task<List<IdeaDto>> GetIdeesUtilisateurAsync(int userId);
    Task<List<IdeaDto>> GetIdeesTeamAsync(int teamId, int userId);
    Task<List<IdeaDto>> GetIdeesPubliquesAsync();
    Task<IdeaDto?> GetIdeeByIdAsync(int id, int userId);
    Task<bool> SupprimerIdeeAsync(int id, int userId);
}
