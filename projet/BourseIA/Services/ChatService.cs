using BourseIA.Data;
using BourseIA.DTOs;
using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db) => _db = db;

    public async Task<ChatMessageDto> EnvoyerMessageAsync(EnvoyerMessageDto dto, int expediteurId)
    {
        if (dto.DestinataireId is null && dto.TeamId is null)
            throw new InvalidOperationException("Veuillez spécifier un destinataire ou une équipe.");

        if (dto.TeamId.HasValue)
        {
            var estMembre = await _db.MembresEquipe.AnyAsync(m =>
                m.TeamId == dto.TeamId && m.UtilisateurId == expediteurId && m.Statut == "Accepté");
            if (!estMembre)
                throw new UnauthorizedAccessException("Vous n'êtes pas membre de cette équipe.");
        }

        var message = new MessageChat
        {
            Contenu = dto.Contenu,
            ExpediteurId = expediteurId,
            DestinataireId = dto.DestinataireId,
            TeamId = dto.TeamId
        };

        _db.MessagesChat.Add(message);
        await _db.SaveChangesAsync();

        var expediteur = await _db.Utilisateurs.FindAsync(expediteurId);
        return MapToDto(message, expediteur!);
    }

    public async Task<List<ChatMessageDto>> GetConversationPriveeAsync(int userId, int contactId)
    {
        var messages = await _db.MessagesChat
            .Include(m => m.Expediteur)
            .Where(m => m.TeamId == null &&
                ((m.ExpediteurId == userId && m.DestinataireId == contactId) ||
                 (m.ExpediteurId == contactId && m.DestinataireId == userId)))
            .OrderBy(m => m.DateEnvoi)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, m.Expediteur)).ToList();
    }

    public async Task<List<ChatMessageDto>> GetMessagesTeamAsync(int teamId, int userId)
    {
        var estMembre = await _db.MembresEquipe.AnyAsync(m =>
            m.TeamId == teamId && m.UtilisateurId == userId && m.Statut == "Accepté");
        if (!estMembre) return [];

        var messages = await _db.MessagesChat
            .Include(m => m.Expediteur)
            .Where(m => m.TeamId == teamId)
            .OrderBy(m => m.DateEnvoi)
            .ToListAsync();

        return messages.Select(m => MapToDto(m, m.Expediteur)).ToList();
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
    {
        var contactIds = await _db.MessagesChat
            .Where(m => m.TeamId == null &&
                (m.ExpediteurId == userId || m.DestinataireId == userId))
            .Select(m => m.ExpediteurId == userId ? m.DestinataireId!.Value : m.ExpediteurId)
            .Distinct()
            .ToListAsync();

        var conversations = new List<ConversationDto>();
        foreach (var contactId in contactIds)
        {
            var contact = await _db.Utilisateurs.FindAsync(contactId);
            if (contact is null) continue;

            var dernier = await _db.MessagesChat
                .Where(m => m.TeamId == null &&
                    ((m.ExpediteurId == userId && m.DestinataireId == contactId) ||
                     (m.ExpediteurId == contactId && m.DestinataireId == userId)))
                .OrderByDescending(m => m.DateEnvoi)
                .FirstOrDefaultAsync();

            var nonLus = await _db.MessagesChat.CountAsync(m =>
                m.ExpediteurId == contactId && m.DestinataireId == userId && !m.EstLu);

            conversations.Add(new ConversationDto
            {
                UtilisateurId = contactId,
                Nom = contact.Nom,
                Prenom = contact.Prenom,
                PhotoProfil = contact.PhotoProfil,
                DernierMessage = dernier?.Contenu,
                DateDernierMessage = dernier?.DateEnvoi,
                MessagesNonLus = nonLus
            });
        }

        return conversations.OrderByDescending(c => c.DateDernierMessage).ToList();
    }

    public async Task MarquerCommeLuAsync(int userId, int? contactId, int? teamId)
    {
        List<MessageChat> messages;

        if (teamId.HasValue)
        {
            messages = await _db.MessagesChat
                .Where(m => m.TeamId == teamId && m.ExpediteurId != userId && !m.EstLu)
                .ToListAsync();
        }
        else if (contactId.HasValue)
        {
            messages = await _db.MessagesChat
                .Where(m => m.ExpediteurId == contactId && m.DestinataireId == userId && !m.EstLu)
                .ToListAsync();
        }
        else return;

        messages.ForEach(m => m.EstLu = true);
        await _db.SaveChangesAsync();
    }

    private static ChatMessageDto MapToDto(MessageChat m, Utilisateur expediteur) => new()
    {
        Id = m.Id,
        Contenu = m.Contenu,
        DateEnvoi = m.DateEnvoi,
        EstLu = m.EstLu,
        TypeMessage = m.TypeMessage,
        ExpediteurId = m.ExpediteurId,
        NomExpediteur = $"{expediteur.Prenom} {expediteur.Nom}",
        DestinataireId = m.DestinataireId,
        TeamId = m.TeamId
    };
}
