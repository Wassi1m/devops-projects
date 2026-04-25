using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class Notification
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "Info";

    public bool EstLue { get; set; } = false;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public string? LienAction { get; set; }

    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; } = null!;
}
