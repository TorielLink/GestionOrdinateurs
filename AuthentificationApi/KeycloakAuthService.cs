using System.Net;
using System.Net.Http.Json;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi;

// Échange login/mot de passe contre un token Keycloak
public sealed class KeycloakAuthService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAuthService> logger)
{
    // Renvoie le token, ou null si Keycloak refuse les identifiants
    public async Task<KeycloakTokenResponse?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var realm = configuration["Keycloak:Realm"]
            ?? throw new InvalidOperationException("Keycloak:Realm manquant dans appsettings.json");
        var clientId = configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Keycloak:ClientId manquant dans appsettings.json");
        var clientSecret = configuration["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("Keycloak:ClientSecret manquant dans appsettings.json");

        // Corps de la requête au format application/x-www-form-urlencoded, attendu par Keycloak
        using var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["username"] = username,
            ["password"] = password
        });

        using var response = await httpClient.PostAsync(
            $"realms/{realm}/protocol/openid-connect/token", body, cancellationToken);

        // 400/401 : identifiants refusés (invalid_grant) ou client mal configuré (unauthorized_client)
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            var erreur = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Connexion Keycloak refusée pour {Username} : {Erreur}", username, erreur);
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
    }
}