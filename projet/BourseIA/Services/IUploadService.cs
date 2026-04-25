using BourseIA.DTOs;

namespace BourseIA.Services;

public interface IUploadService
{
    Task<CourbeDto> UploadCourbeAsync(IFormFile fichier, string? typeAction, int userId);
    Task<List<CourbeDto>> GetCourbesUtilisateurAsync(int userId);
    Task<CourbeDto?> GetCourbeByIdAsync(int courbeId, int userId);
    Task<bool> SupprimerCourbeAsync(int courbeId, int userId);
}
