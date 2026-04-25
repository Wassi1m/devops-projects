using BourseIA.DTOs;
using BourseIA.Services;
using BourseIA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BourseIA.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly JwtHelper _jwtHelper;

    public ChatHub(IChatService chatService, JwtHelper jwtHelper)
    {
        _chatService = chatService;
        _jwtHelper = jwtHelper;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnConnectedAsync();
    }

    public async Task RejoindreGroupeEquipe(int teamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task QuitterGroupeEquipe(int teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"team_{teamId}");
    }

    public async Task EnvoyerMessagePrive(int destinataireId, string contenu)
    {
        var expediteurId = GetUserId();
        if (!expediteurId.HasValue) return;

        var dto = new EnvoyerMessageDto { Contenu = contenu, DestinataireId = destinataireId };
        var message = await _chatService.EnvoyerMessageAsync(dto, expediteurId.Value);

        await Clients.Group($"user_{destinataireId}").SendAsync("NouveauMessage", message);
        await Clients.Caller.SendAsync("NouveauMessage", message);
    }

    public async Task EnvoyerMessageEquipe(int teamId, string contenu)
    {
        var expediteurId = GetUserId();
        if (!expediteurId.HasValue) return;

        try
        {
            var dto = new EnvoyerMessageDto { Contenu = contenu, TeamId = teamId };
            var message = await _chatService.EnvoyerMessageAsync(dto, expediteurId.Value);

            await Clients.Group($"team_{teamId}").SendAsync("NouveauMessageEquipe", message);
        }
        catch (UnauthorizedAccessException)
        {
            await Clients.Caller.SendAsync("Erreur", "Vous n'êtes pas membre de cette équipe.");
        }
    }

    private int? GetUserId()
    {
        var idClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return idClaim is not null ? int.Parse(idClaim) : null;
    }
}
