using BourseIA.DTOs;
using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IdeaController : ControllerBase
{
    private readonly IIdeaService _ideaService;

    public IdeaController(IIdeaService ideaService) => _ideaService = ideaService;

    [HttpPost]
    public async Task<IActionResult> CreerIdee([FromBody] CreateIdeaDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var idee = await _ideaService.CreerIdeeAsync(dto, userId);
        return CreatedAtAction(nameof(GetIdee), new { id = idee.Id }, idee);
    }

    [HttpGet("mes-idees")]
    public async Task<IActionResult> GetMesIdees()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var idees = await _ideaService.GetIdeesUtilisateurAsync(userId);
        return Ok(idees);
    }

    [HttpGet("equipe/{teamId}")]
    public async Task<IActionResult> GetIdeesEquipe(int teamId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var idees = await _ideaService.GetIdeesTeamAsync(teamId, userId);
        return Ok(idees);
    }

    [HttpGet("publiques")]
    public async Task<IActionResult> GetIdeesPubliques()
    {
        var idees = await _ideaService.GetIdeesPubliquesAsync();
        return Ok(idees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIdee(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var idee = await _ideaService.GetIdeeByIdAsync(id, userId);
        return idee is null ? NotFound() : Ok(idee);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SupprimerIdee(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _ideaService.SupprimerIdeeAsync(id, userId);
        return success ? NoContent() : NotFound();
    }
}
