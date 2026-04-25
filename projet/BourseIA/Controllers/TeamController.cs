using BourseIA.DTOs;
using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService) => _teamService = teamService;

    [HttpPost]
    public async Task<IActionResult> CreerTeam([FromBody] CreateTeamDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var team = await _teamService.CreerTeamAsync(dto, userId);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpGet]
    public async Task<IActionResult> GetMesTeams()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teams = await _teamService.GetTeamsUtilisateurAsync(userId);
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeam(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var team = await _teamService.GetTeamDetailAsync(id, userId);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost("inviter")]
    public async Task<IActionResult> InviterMembre([FromBody] InvitationDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.InviterMembreAsync(dto, userId);
        return success ? Ok(new { message = "Invitation envoyée." }) : BadRequest(new { message = "Impossible d'inviter ce membre." });
    }

    [HttpPost("invitation/repondre")]
    public async Task<IActionResult> RepondreInvitation([FromBody] RepondreInvitationDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.RepondreInvitationAsync(dto, userId);
        return success ? Ok(new { message = dto.Accepter ? "Invitation acceptée." : "Invitation refusée." })
                       : NotFound(new { message = "Invitation introuvable." });
    }

    [HttpPost("partager")]
    public async Task<IActionResult> PartagerCourbe([FromBody] PartagerCourbeDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.PartagerCourbeAsync(dto, userId);
        return success ? Ok(new { message = "Courbe partagée avec l'équipe." })
                       : BadRequest(new { message = "Impossible de partager cette courbe." });
    }

    [HttpDelete("{id}/quitter")]
    public async Task<IActionResult> QuitterTeam(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.QuitterTeamAsync(id, userId);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SupprimerTeam(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.SupprimerTeamAsync(id, userId);
        return success ? NoContent() : NotFound(new { message = "Équipe introuvable ou accès refusé." });
    }

    [HttpDelete("{id}/membre/{membreId}/kick")]
    public async Task<IActionResult> KickerMembre(int id, int membreId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.KickerMembreAsync(id, membreId, userId);
        return success ? Ok(new { message = "Membre retiré de l'équipe." })
                       : BadRequest(new { message = "Action impossible. Vérifiez vos droits." });
    }

    [HttpPost("{id}/membre/{membreId}/ban")]
    public async Task<IActionResult> BannerMembre(int id, int membreId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.BannerMembreAsync(id, membreId, userId);
        return success ? Ok(new { message = "Membre banni de l'équipe." })
                       : BadRequest(new { message = "Action impossible. Vérifiez vos droits." });
    }

    [HttpPost("{id}/membre/{membreId}/debloquer")]
    public async Task<IActionResult> DebloquerMembre(int id, int membreId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.DebloquerMembreAsync(id, membreId, userId);
        return success ? Ok(new { message = "Membre débloqué." })
                       : BadRequest(new { message = "Action impossible. Vérifiez vos droits." });
    }

    [HttpPut("{id}/membre/{membreId}/role")]
    public async Task<IActionResult> ChangerRole(int id, int membreId, [FromBody] ChangerRoleDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.ChangerRoleAsync(id, membreId, dto.NouveauRole, userId);
        return success ? Ok(new { message = "Rôle mis à jour." })
                       : BadRequest(new { message = "Action impossible. Seul le créateur peut changer les rôles." });
    }

    [HttpGet("invitations")]
    public async Task<IActionResult> GetMesInvitations()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invitations = await _teamService.GetInvitationsEnAttenteAsync(userId);
        return Ok(invitations);
    }

    [HttpGet("{id}/courbes")]
    public async Task<IActionResult> GetCourbsEquipe(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var courbes = await _teamService.GetCourbsEquipeAsync(id, userId);
        return Ok(courbes);
    }

    [HttpDelete("{id}/courbes/{courbeId}")]
    public async Task<IActionResult> RetirerPartage(int id, int courbeId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _teamService.RetirerPartageAsync(id, courbeId, userId);
        return success ? NoContent() : NotFound(new { message = "Partage introuvable ou accès refusé." });
    }
}
