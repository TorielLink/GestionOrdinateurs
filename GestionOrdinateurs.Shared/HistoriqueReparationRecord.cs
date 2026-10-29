namespace GestionOrdinateurs.Shared;

// Record partagé entre l'API (qui le renvoie) et le client Blazor (qui l'affiche)
public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention
);