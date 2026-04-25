using BourseIA.DTOs;
using BourseIA.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BourseIA.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _userService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _userService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("profil")]
    public async Task<IActionResult> GetProfil()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profil = await _userService.GetByIdAsync(userId);
        return profil is null ? NotFound() : Ok(profil);
    }

    [Authorize]
    [HttpPut("profil")]
    public async Task<IActionResult> UpdateProfil([FromBody] UpdateProfilDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.UpdateProfilAsync(userId, dto);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("profil/photo")]
    public async Task<IActionResult> UpdatePhoto(IFormFile photo)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var url = await _userService.UpdatePhotoProfilAsync(userId, photo);
        return Ok(new { photoUrl = url });
    }

    [Authorize]
    [HttpGet("tableau-de-bord")]
    public async Task<IActionResult> GetTableauDeBord()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var stats = await _userService.GetStatistiquesAsync(userId);
        return Ok(stats);
    }
}
