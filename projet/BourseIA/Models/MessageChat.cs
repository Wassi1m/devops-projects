using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class MessageChat
{
    public int Id { get; set; }

    [Required]
    public string Contenu { get; set; } = string.Empty;

    public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;

    public bool EstLu { get; set; } = false;

    public string TypeMessage { get; set; } = "Texte";

    public int ExpediteurId { get; set; }
    public Utilisateur Expediteur { get; set; } = null!;

    public int? DestinataireId { get; set; }
    public Utilisateur? Destinataire { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }
}
