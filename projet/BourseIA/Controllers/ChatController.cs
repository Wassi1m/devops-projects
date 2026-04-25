using BourseIA.DTOs;
using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService) => _chatService = chatService;

    [HttpPost("envoyer")]
    public async Task<IActionResult> EnvoyerMessage([FromBody] EnvoyerMessageDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var message = await _chatService.EnvoyerMessageAsync(dto, userId);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conversations = await _chatService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("prive/{contactId}")]
    public async Task<IActionResult> GetConversationPrivee(int contactId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var messages = await _chatService.GetConversationPriveeAsync(userId, contactId);
        return Ok(messages);
    }

    [HttpGet("equipe/{teamId}")]
    public async Task<IActionResult> GetMessagesEquipe(int teamId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var messages = await _chatService.GetMessagesTeamAsync(teamId, userId);
        return Ok(messages);
    }

    [HttpPost("marquer-lu")]
    public async Task<IActionResult> MarquerLu([FromQuery] int? contactId, [FromQuery] int? teamId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _chatService.MarquerCommeLuAsync(userId, contactId, teamId);
        return Ok(new { message = "Messages marqués comme lus." });
    }
}
