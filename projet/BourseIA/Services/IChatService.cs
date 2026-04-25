using BourseIA.DTOs;

namespace BourseIA.Services;

public interface IChatService
{
    Task<ChatMessageDto> EnvoyerMessageAsync(EnvoyerMessageDto dto, int expediteurId);
    Task<List<ChatMessageDto>> GetConversationPriveeAsync(int userId, int contactId);
    Task<List<ChatMessageDto>> GetMessagesTeamAsync(int teamId, int userId);
    Task<List<ConversationDto>> GetConversationsAsync(int userId);
    Task MarquerCommeLuAsync(int userId, int? contactId, int? teamId);
}
