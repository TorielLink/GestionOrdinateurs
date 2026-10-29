using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class ReparationService(
    ParcInformatiqueContext context,
    IDbContextFactory<ParcInformatiqueContext> factory,
    JournalService journal)
    : IReparationService
{
    public async Task SimulerReparationAsync(Ordinateur ordinateur, string technicien)
    {
        // je crée un DbContext par réparation via la fabrique parce qu'un DbContext n'est pas thread-safe
        // et garde un état (change tracker) ; un lock empêcherait l'accès simultané, mais supprimerait
        // le parallélisme et donc mélangerait les états
        // un contexte par réparation garantit des opérations réellement parallèles et isolées
        await using var contextTmp = await factory.CreateDbContextAsync();

        // Création object reparation (test)
        var reparation = new Reparation
        {
            OrdinateurId = ordinateur.Id, // je fais passer la clef étrangère plutôt qu'un ordinateur
            TypeReparation = "Maintenance",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = technicien
        };

        await contextTmp.Reparations.AddAsync(reparation);
        await contextTmp.SaveChangesAsync();

        await Task.Delay(2000);

        reparation.Statut = "Terminée";
        await contextTmp.SaveChangesAsync();

        Console.WriteLine($"{ordinateur.Nom} réparé par {technicien}");
        
        lock (journal)
        {
            journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} réparé");
        }
    }

    public async Task<List<Reparation>> ListerReparationsInfos()
    {
        // On récupère la liste de toutes les réparations
        List<Reparation> reparations = await ListerReparations();
        List<Reparation> reparationsAvecInfos = new(); // la liste qui aura toutes les infos

        // Et pour chaque réparation, on récupère toutes ses infos
        foreach (Reparation reparation in reparations)
        {
            Reparation? info = await RecupererInfoReparation(reparation.Id);

            if (info != null)
                reparationsAvecInfos.Add(info);
        }

        return reparationsAvecInfos;
    }

    public async Task<List<Reparation>> ListerReparations()
    {
        return await context.Reparations.ToListAsync();
    }

    public async Task<Reparation?> RecupererInfoReparation(int id)
    {
        // "Include" charge l'ordinateur associé à la réparation
        return await context.Reparations
            .Include(r => r.UnOrdinateur)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync()
    {
        return await context.Reparations
            .OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(
                r.OrdinateurId,
                r.UnOrdinateur.Nom,
                r.Technicien,
                r.TypeReparation,
                r.DateDebut))
            .ToListAsync();
    }
}