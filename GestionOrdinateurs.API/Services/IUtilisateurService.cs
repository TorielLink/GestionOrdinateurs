using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services;

public interface IUtilisateurService
{
    Task<List<Utilisateur>> ListerUtilisateurs();

    Task<Utilisateur?> ObtenirParLogin(string login);
}