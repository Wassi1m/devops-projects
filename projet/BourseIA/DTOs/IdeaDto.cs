using System.ComponentModel.DataAnnotations;

namespace BourseIA.DTOs;

public class CreateIdeaDto
{
    [Required, MaxLength(200)]
    public string Titre { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? ActionConcernee { get; set; }
    public string Tendance { get; set; } = "Neutre";
    public double? RentabiliteEstimee { get; set; }
    public bool EstPublique { get; set; } = false;
    public int? TeamId { get; set; }
    public int? ResultatAnalyseId { get; set; }
}

public class IdeaDto
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ActionConcernee { get; set; }
    public string TypeIdee { get; set; } = string.Empty;
    public string Tendance { get; set; } = string.Empty;
    public double? RentabiliteEstimee { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstPublique { get; set; }
    public int UtilisateurId { get; set; }
    public string NomAuteur { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string? NomTeam { get; set; }
}
