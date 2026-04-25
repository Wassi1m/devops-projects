using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BourseIA.Models;

public class Utilisateur
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasseHash { get; set; } = string.Empty;

    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public string? PhotoProfil { get; set; }

    public string ProfilInvestisseur { get; set; } = "Débutant";

    public ICollection<CourbeBoursiere> Courbes { get; set; } = new List<CourbeBoursiere>();
    public ICollection<IdeeInvestissement> Idees { get; set; } = new List<IdeeInvestissement>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<MessageChat> MessagesEnvoyes { get; set; } = new List<MessageChat>();
    public ICollection<MembreEquipe> Equipes { get; set; } = new List<MembreEquipe>();
}
