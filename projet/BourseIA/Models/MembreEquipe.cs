namespace BourseIA.Models;

public class MembreEquipe
{
    public int Id { get; set; }

    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public string Role { get; set; } = "Membre";

    public DateTime DateAdhesion { get; set; } = DateTime.UtcNow;

    public string Statut { get; set; } = "EnAttente";
}
