using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class UtilisateurService(ParcInformatiqueContext context) : IUtilisateurService
{
    // Liste tous les utilisateurs de la base
    public async Task<List<Utilisateur>> ListerUtilisateurs()
    {
        return await context.Utilisateurs.ToListAsync();
    }

    // Récupère l'utilisateur au login donné
    public async Task<Utilisateur?> ObtenirParLogin(string login)
    {
        return await context.Utilisateurs.FirstOrDefaultAsync(u => u.Login == login);
    }
}