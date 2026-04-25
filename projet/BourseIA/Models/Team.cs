using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class Team
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public int CreateurId { get; set; }
    public Utilisateur Createur { get; set; } = null!;

    public ICollection<MembreEquipe> Membres { get; set; } = new List<MembreEquipe>();
    public ICollection<MessageChat> Messages { get; set; } = new List<MessageChat>();
    public ICollection<PartageEquipe> Partages { get; set; } = new List<PartageEquipe>();
    public ICollection<IdeeInvestissement> Idees { get; set; } = new List<IdeeInvestissement>();
}
