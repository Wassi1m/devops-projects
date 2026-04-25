using BourseIA.DTOs;
using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IUploadService _uploadService;

    public UploadController(IUploadService uploadService) => _uploadService = uploadService;

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var result = await _uploadService.UploadCourbeAsync(dto.Fichier, dto.TypeAction, userId);
            return CreatedAtAction(nameof(GetCourbe), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMesCourbes()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var courbes = await _uploadService.GetCourbesUtilisateurAsync(userId);
        return Ok(courbes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourbe(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var courbe = await _uploadService.GetCourbeByIdAsync(id, userId);
        return courbe is null ? NotFound() : Ok(courbe);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SupprimerCourbe(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _uploadService.SupprimerCourbeAsync(id, userId);
        return success ? NoContent() : NotFound();
    }
}
