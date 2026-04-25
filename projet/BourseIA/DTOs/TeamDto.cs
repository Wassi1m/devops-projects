using System.ComponentModel.DataAnnotations;

namespace BourseIA.DTOs;

public class CreateTeamDto
{
    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class TeamDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreation { get; set; }
    public int CreateurId { get; set; }
    public string NomCreateur { get; set; } = string.Empty;
    public int NombreMembres { get; set; }
    public string MonRole { get; set; } = string.Empty;
}

public class TeamDetailDto : TeamDto
{
    public List<MembreEquipeDto> Membres { get; set; } = new();
    public List<CourbeDto> Courbes { get; set; } = new();
}

public class MembreEquipeDto
{
    public int UtilisateurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public DateTime DateAdhesion { get; set; }
}

public class InvitationDto
{
    [Required]
    public int TeamId { get; set; }

    public string? EmailInvite { get; set; }
    public int? UtilisateurId { get; set; }
}

public class RepondreInvitationDto
{
    [Required]
    public int TeamId { get; set; }

    [Required]
    public bool Accepter { get; set; }
}

public class PartagerCourbeDto
{
    [Required]
    public int TeamId { get; set; }

    [Required]
    public int CourbeId { get; set; }

    public string? Commentaire { get; set; }
}

public class GestionMembreDto
{
    [Required]
    public int MembreId { get; set; }
}

public class ChangerRoleDto
{
    [Required]
    public int MembreId { get; set; }

    [Required, MaxLength(50)]
    public string NouveauRole { get; set; } = string.Empty;
}

public class CourbePartageDto
{
    public int PartageId { get; set; }
    public int CourbeId { get; set; }
    public string NomFichier { get; set; } = string.Empty;
    public string? TypeAction { get; set; }
    public DateTime DateUpload { get; set; }
    public string Statut { get; set; } = string.Empty;
    public int PartageParId { get; set; }
    public string NomPartage { get; set; } = string.Empty;
    public string? Commentaire { get; set; }
    public DateTime DatePartage { get; set; }
    public AnalysisResultDto? Analyse { get; set; }
}

public class InvitationEnAttenteDto
{
    public int TeamId { get; set; }
    public string TeamNom { get; set; } = string.Empty;
    public string CreateurNom { get; set; } = string.Empty;
    public DateTime DateInvitation { get; set; }
}
