using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/auth")]
public class KeycloakAuthController(KeycloakAuthService keycloakAuthService) : ControllerBase
{
    // POST api/auth/login : login + mot de passe, renvoie le token Keycloak
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginKeycloakRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Le login et le mot de passe sont obligatoires.");

        var token = await keycloakAuthService.LoginAsync(request.Username.Trim(), request.Password, cancellationToken);

        return token is null
            ? Unauthorized("Identifiant ou mot de passe incorrect.")
            : Ok(token);
    }
}