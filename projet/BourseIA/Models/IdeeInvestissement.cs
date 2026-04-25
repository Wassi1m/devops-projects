using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class IdeeInvestissement
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ActionConcernee { get; set; }

    public string TypeIdee { get; set; } = "Manuelle";

    public string Tendance { get; set; } = "Neutre";

    public double? RentabiliteEstimee { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public bool EstPublique { get; set; } = false;

    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; } = null!;

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public int? ResultatAnalyseId { get; set; }
    public ResultatAnalyse? ResultatAnalyse { get; set; }
}
