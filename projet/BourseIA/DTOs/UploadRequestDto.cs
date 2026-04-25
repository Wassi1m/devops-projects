using System.ComponentModel.DataAnnotations;

namespace BourseIA.DTOs;

public class UploadRequestDto
{
    [Required]
    public IFormFile Fichier { get; set; } = null!;

    public string? TypeAction { get; set; }
}

public class CourbeDto
{
    public int Id { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public string? TypeAction { get; set; }
    public DateTime DateUpload { get; set; }
    public string Statut { get; set; } = string.Empty;
    public int UtilisateurId { get; set; }
    public string NomUtilisateur { get; set; } = string.Empty;
    public AnalysisResultDto? Resultat { get; set; }
}
