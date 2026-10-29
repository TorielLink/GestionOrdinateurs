using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    // POST api/authentication
    [HttpPost]
    public IActionResult Login(RoleEnum role, string login)
        // Génère un token JWT à partir du login et du rôle de l'utilisateur
    {
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest("Le login est obligatoire.");

        var jwtKey = _configuration["Jwt:Key"] ?? "";
        var key = Encoding.ASCII.GetBytes(jwtKey);

        return Ok(GenererTokenJwt(key, role, login));
    }
    
    private string GenererTokenJwt(byte[] cle, RoleEnum role, string login)
        // Génère le token JWT (valable 15 minutes)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // login et rôle stockés dans le token sous forme de claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(cle),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}