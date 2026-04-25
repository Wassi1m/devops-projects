using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class CourbeBoursiere
{
    public int Id { get; set; }

    [Required]
    public string NomFichier { get; set; } = string.Empty;

    [Required]
    public string CheminFichier { get; set; } = string.Empty;

    public string? TypeAction { get; set; }

    public DateTime DateUpload { get; set; } = DateTime.UtcNow;

    public string Statut { get; set; } = "EnAttente";

    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; } = null!;

    public ResultatAnalyse? ResultatAnalyse { get; set; }

    public ICollection<PartageEquipe> Partages { get; set; } = new List<PartageEquipe>();
}
