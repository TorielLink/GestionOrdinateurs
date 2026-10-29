using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services;

public interface IReparationService
{
    Task SimulerReparationAsync(Ordinateur ordinateur, string technicien);

    Task<List<Reparation>> ListerReparationsInfos();
    
    Task<List<Reparation>> ListerReparations();

    Task<Reparation?> RecupererInfoReparation(int id);
    
    Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync();
}