using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services;

public interface IOrdinateurService
{
    event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;

    event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;

    event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;
    
    void Ajouter(Ordinateur ordinateur);

    bool Supprimer(int id);

    bool MettreAJour(Ordinateur ordinateur);

    Task<List<Ordinateur>> Lister();

    Task<Ordinateur?> ObtenirParId(int id);

    Task<List<Ordinateur>> ObtenirParMarque(string marque);
    
    Task<List<Ordinateur>> ObtenirDisponibles();
    
    Task<List<Ordinateur>> ObtenirParPrixDecroissant();

    string Diagnostiquer(Ordinateur ordinateur);
    
    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();
}