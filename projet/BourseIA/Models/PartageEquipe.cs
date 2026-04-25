namespace BourseIA.Models;

public class PartageEquipe
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int CourbeBoursiereId { get; set; }
    public CourbeBoursiere CourbeBoursiere { get; set; } = null!;

    public int PartageParId { get; set; }
    public Utilisateur PartagePar { get; set; } = null!;

    public DateTime DatePartage { get; set; } = DateTime.UtcNow;

    public string? Commentaire { get; set; }
}
