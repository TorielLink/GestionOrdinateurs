using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class OrdinateurService(ParcInformatiqueContext context,
    JournalService journal) : IOrdinateurService
{
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;

    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;

    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
    
    // Fonction d'ajout d'un ordinateur au parc d'ordinateurs
    public void Ajouter(Ordinateur ordinateur)
    {
        context.Ordinateurs.Add(ordinateur);
        context.SaveChanges();

        lock (journal)
        {
            journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} ajouté");
        }

        // Déclenche l'événement après l'ajout en base
        OrdinateurAjoute?.Invoke(
            this,
            new OrdinateurAjouteEventArgs(ordinateur));
    }

    // Fonction de suppression d'un ordinateur du parc d'ordinateurs
    public bool Supprimer(int id)
    {
        // Pour récupérer la première occurence au cas où l'unicité a été bafouée
        Ordinateur? ordinateur = context.Ordinateurs.FirstOrDefault(o => o.Id == id);

        if (ordinateur == null)
            return false;

        context.Ordinateurs.Remove(ordinateur);
        context.SaveChanges();

        lock (journal)
        {
            journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} supprimé");
        }

        // Déclenche l'événement après la suppression
        OrdinateurSupprime?.Invoke(
            this,
            new OrdinateurSupprimeEventArgs(ordinateur));
        
        return true;
    }

    // Fonction de mise à jour d'un ordinateur du parc d'ordinateurs
    public bool MettreAJour(Ordinateur ordinateur)
    {
        // Pour récupérer la première occurence au cas où l'unicité a été bafouée
        Ordinateur? existant = context.Ordinateurs
            .FirstOrDefault(o => o.Id == ordinateur.Id);

        if (existant == null)
            return false;

        context.Ordinateurs.Remove(existant);
        context.Ordinateurs.Add(ordinateur);
        context.SaveChanges();

        lock (journal)
        {
            journal.Ecrire($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} modifié");
        }

        // Déclenche l'événement après la mise à jour
        OrdinateurMisAJour?.Invoke(
            this,
            new OrdinateurMisAJourEventArgs(ordinateur));

        return true;
    }

    // Fonction de listage des ordinateurs du parc d'ordinateurs
    public async Task<List<Ordinateur>> Lister()
    {
        return await context.Ordinateurs.ToListAsync();
    }

    // Fonction de récupération d'un ordinateur du parc d'ordinateurs par son identifiant
    public async Task<Ordinateur?> ObtenirParId(int id)
    {
        // Pour récupérer la première occurrence au cas où l'unicité a été bafouée
        return await context.Ordinateurs
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    // Fonction de récupération des ordinateurs du parc d'une marque
    public async Task<List<Ordinateur>> ObtenirParMarque(string marque)
    {
        return await context.Ordinateurs
            .Where(o => o.Caracteristiques.Marque.ToLower() == marque.ToLower())
            .ToListAsync();
    }

    public async Task<List<Ordinateur>> ObtenirDisponibles()
    {
        return await context.Ordinateurs
            .Where(o => o.Etat.Equals(EtatsOrdinateur.Disponible) && o.Stock > 0)
            .ToListAsync();
    }

    public async Task<List<Ordinateur>> ObtenirParPrixDecroissant()
    {
        List<Ordinateur> ordinateurs = await context.Ordinateurs.ToListAsync();

        return ordinateurs
            .OrderByDescending(o => o.Prix)
            .ToList();
    }
    
    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            {
                    Etat: EtatsOrdinateur.HorsService
                }
                => "Remplacement",

            {
                    Etat: EtatsOrdinateur.Lent,
                    Caracteristiques:
                    {
                        RamGo: < 8
                    }
                }
                => "Ajouter RAM",

            _
                => "Aucune action"
        };
    }

    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques()
    {
        // On récupère tous les ordinateurs 
        List<Ordinateur> ordinateurs = context.Ordinateurs.ToList();

        int total = ordinateurs.Count;

        // ordinateurs avec état "Disponible"
        int disponibles = ordinateurs.Count(o => o.Etat == EtatsOrdinateur.Disponible);

        // valeur stock = somme des prix * stock de chaque ordi
        decimal valeurStock = ordinateurs.Sum(o => o.Prix * o.Stock);
        
        int nbReparations = context.Reparations.Count();

        return (total, disponibles, valeurStock, nbReparations);
    }

}