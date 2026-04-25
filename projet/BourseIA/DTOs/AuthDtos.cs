using System.ComponentModel.DataAnnotations;

namespace BourseIA.DTOs;

public class RegisterDto
{
    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string MotDePasse { get; set; } = string.Empty;

    public string ProfilInvestisseur { get; set; } = "Débutant";
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasse { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UtilisateurDto Utilisateur { get; set; } = null!;
}

public class UtilisateurDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfilInvestisseur { get; set; } = string.Empty;
    public string? PhotoProfil { get; set; }
    public DateTime DateInscription { get; set; }
}

public class UpdateProfilDto
{
    [MaxLength(100)]
    public string? Nom { get; set; }

    [MaxLength(100)]
    public string? Prenom { get; set; }

    public string? ProfilInvestisseur { get; set; }
}
