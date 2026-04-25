using System.ComponentModel.DataAnnotations;

namespace BourseIA.DTOs;

public class EnvoyerMessageDto
{
    [Required]
    public string Contenu { get; set; } = string.Empty;

    public int? DestinataireId { get; set; }
    public int? TeamId { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Contenu { get; set; } = string.Empty;
    public DateTime DateEnvoi { get; set; }
    public bool EstLu { get; set; }
    public string TypeMessage { get; set; } = string.Empty;
    public int ExpediteurId { get; set; }
    public string NomExpediteur { get; set; } = string.Empty;
    public int? DestinataireId { get; set; }
    public int? TeamId { get; set; }
}

public class ConversationDto
{
    public int UtilisateurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? PhotoProfil { get; set; }
    public string? DernierMessage { get; set; }
    public DateTime? DateDernierMessage { get; set; }
    public int MessagesNonLus { get; set; }
}
