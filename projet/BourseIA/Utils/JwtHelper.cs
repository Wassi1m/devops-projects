using BourseIA.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BourseIA.Utils;

public class JwtHelper
{
    private readonly IConfiguration _config;

    public JwtHelper(IConfiguration config) => _config = config;

    public (string Token, DateTime Expiration) GenererToken(Utilisateur utilisateur)
    {
        var cle = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Cle"]!));
        var credentials = new SigningCredentials(cle, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:DureeHeures"] ?? "24"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
            new Claim(ClaimTypes.Email, utilisateur.Email),
            new Claim(ClaimTypes.Name, $"{utilisateur.Prenom} {utilisateur.Nom}"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Emetteur"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    public int? ExtraireUserId(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var cle = Encoding.UTF8.GetBytes(_config["Jwt:Cle"]!);
            var parametres = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(cle),
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Emetteur"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true
            };

            var principal = handler.ValidateToken(token, parametres, out _);
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return idClaim is not null ? int.Parse(idClaim) : null;
        }
        catch
        {
            return null;
        }
    }
}
