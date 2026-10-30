using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Utils;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;

    public JwtAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetItemAsync<string>("jwtToken");
        var identity = new ClaimsIdentity();

        if (!string.IsNullOrWhiteSpace(token))
        {
            var claims = ParseClaimsFromJwt(token);
            identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    // À appeler après la connexion
    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        var claims = new List<Claim>();

        foreach (var claim in token.Claims)
        {
            switch (claim.Type)
            {
                case "role":
                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    break;
                case "unique_name":
                    claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    break;
                case "roles":
                    claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                    break;
                case "preferred_username":
                    claims.Add(new Claim(ClaimTypes.Name, claim.Value));
                    break;
                default:
                    claims.Add(claim);
                    break;
            }
        }

        return claims;
    }
}