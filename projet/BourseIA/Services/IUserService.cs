using BourseIA.DTOs;

namespace BourseIA.Services;

public interface IUserService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UtilisateurDto?> GetByIdAsync(int id);
    Task<UtilisateurDto> UpdateProfilAsync(int id, UpdateProfilDto dto);
    Task<string?> UpdatePhotoProfilAsync(int id, IFormFile photo);
    Task<StatistiquesDto> GetStatistiquesAsync(int userId);
}
