using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // tout le contrôleur nécessite un token valide
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IConfiguration _configuration;

    public OrdinateursController(
        IOrdinateurService service,
        ILogger<OrdinateursController> logger,
        IConfiguration configuration)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;

        service.OrdinateurAjoute += AfficherMessageAjout;
        service.OrdinateurSupprime += AfficherMessageSuppression;
        service.OrdinateurMisAJour += AfficherMessageMiseAJour;
    }

    private void AfficherMessageAjout(
        object? sender,
        OrdinateurAjouteEventArgs e)
    {
        _logger.LogInformation(
            "ÉVÉNEMENT : l'ordinateur {Id} - {Nom} a été ajouté.",
            e.Ordinateur.Id,
            e.Ordinateur.Nom);
    }

    private void AfficherMessageSuppression(
        object? sender,
        OrdinateurSupprimeEventArgs e)
    {
        _logger.LogInformation(
            "ÉVÉNEMENT : l'ordinateur {Id} - {Nom} a été supprimé.",
            e.Ordinateur.Id,
            e.Ordinateur.Nom);
    }

    private void AfficherMessageMiseAJour(
        object? sender,
        OrdinateurMisAJourEventArgs e)
    {
        _logger.LogInformation(
            "ÉVÉNEMENT : l'ordinateur {Id} - {Nom} a été mis à jour.",
            e.Ordinateur.Id,
            e.Ordinateur.Nom);
    }

    // GET /api/ordinateurs
    [HttpGet]
    public async Task<IActionResult> ListerTousLesOrdinateurs()
        // Renvoie la liste des ordinateurs
    {
        _logger.LogInformation("Récupération de la liste des ordinateurs...");
        return Ok(await _service.Lister());
    }

    // GET /api/ordinateurs/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenirOrdinateurParId(int id)
        // Renvoie un ordinateur par son ID
    {
        _logger.LogInformation("Récupération de l'ordinateur avec l'identifiant {Id} ...", id);
        Ordinateur? ordinateur = await _service.ObtenirParId(id);

        if (ordinateur != null) return Ok(ordinateur);
        _logger.LogWarning("Ordinateur avec l'identifiant {Id} introuvable", id);
        return NotFound();
    }

    // GET api/ordinateurs/marque/{marque}
    [HttpGet("/marque/{marque}")]
    public async Task<IActionResult> ObtenirOrdinateurParMarque(string marque)
    {
        // Renvoie les ordinateurs de la marque
        _logger.LogInformation("Récupération des ordinateurs de marque {Marque} ...", marque);

        return Ok(await _service.ObtenirParMarque(marque));
    }

    // GET /api/ordinateurs/disponibles/
    [HttpGet("/disponibles")]
    public async Task<IActionResult> ObtenirOrdinateursDisponible()
    {
        // Renvoie les ordinateurs disponibles et en stock
        _logger.LogInformation("Récupération des ordinateurs disponibles ...");

        return Ok(await _service.ObtenirDisponibles());
    }

    // GET /api/ordinateurs/prix
    [HttpGet("/prix")]
    public async Task<IActionResult> ObtenirOrdinateursParPrixDecroissant()
    {
        // Renvoie les ordinateurs par ordre de prix décroissant
        _logger.LogInformation("Récupération des ordinateurs par ordre de prix décroissant ...");

        return Ok(await _service.ObtenirParPrixDecroissant());
    }

    // GET /api/ordinateurs/info
    [HttpGet("info")]
    public IActionResult Info()
        // Renvoie les informations de l'application
    {
        _logger.LogInformation("Récupération des informations de l'application");
        var nom = _configuration["Application:Name"];
        var version = _configuration["Application:Version"];

        return Ok(new
        {
            Name = nom,
            Version = version
        });
    }

    // GET api/ordinateurs/{id}/diagnostic
    [HttpGet("{id:int}/diagnostic")]
    public async Task<IActionResult> ObtenirDiagnostic(int id)
    {
        _logger.LogInformation("Récupération de l'ordinateur avec l'identifiant {Id} ...", id);
        Ordinateur? ordinateur = await _service.ObtenirParId(id);

        // Renvoie le diagnostic de l'ordinateur id
        _logger.LogInformation("Récupération du diagnostic de l'ordinateur ...");


        if (ordinateur != null) return Ok(_service.Diagnostiquer(ordinateur));

        _logger.LogWarning("Ordinateur avec l'identifiant {Id} introuvable", id);
        return NotFound();
    }

    // GET api/ordinateurs/stats
    [HttpGet("stats")]
    public IActionResult ObtenirStatistiques()
    {
        _logger.LogInformation("Récupération des statistiques ...");

        var (total, disponibles, valeurStock, nbReparations) = _service.CalculerStatistiques();

        return Ok(new
        {
            TotalOrdinateur = total,
            OrdiDisponibles = disponibles,
            ValeurStock = valeurStock,
            NbReparations = nbReparations
        });
    }

    // GET /api/ordinateurs/inventaire/{nom}
    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> Chercher(string nom)
    {
        InventaireOrdinateurs inventaire = new InventaireOrdinateurs();
        List<Ordinateur> ordinateurs = await _service.Lister();
        
        foreach (Ordinateur o in ordinateurs)
        {
            inventaire.Ajouter(o);
        }
        
        _logger.LogInformation("Recherche de l'ordinateur {Nom} dans l'inventaire ...", nom);

        // recherche par nom avec l'indexeur
        Ordinateur? ordinateur = inventaire[nom];

        if (ordinateur == null)
        {
            _logger.LogWarning("Ordinateur {Nom} introuvable dans l'inventaire", nom);
            return NotFound();
        }

        return Ok(ordinateur);
    }


    // POST /api/ordinateurs
    [HttpPost]
    public IActionResult AjouterUnOrdinateur(Ordinateur ordinateur)
        // Ajoute un ordinateur à la liste
    {
        _logger.LogInformation("Ajout de l'ordinateur {Id} de marque {Marque}", ordinateur.Id,
            ordinateur.Caracteristiques?.Marque);

        _service.Ajouter(ordinateur);

        return Ok(ordinateur);
    }

    // PUT /api/ordinateurs/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ModifierInfoOrdinateur(int id, Ordinateur ordinateur)
        // Modifie les informations d'un ordinateur
    {
        _logger.LogInformation($"Mise à jour de l'ordinateur {ordinateur.Id}");
        Ordinateur? existant = await _service.ObtenirParId(id);

        if (existant == null)
        {
            _logger.LogWarning("Ordinateur {OrdinateurId} introuvable", ordinateur.Id);
            return NotFound();
        }

        var resultat = _service.MettreAJour(ordinateur);

        if (resultat) return Ok(ordinateur);
        _logger.LogWarning($"Impossible de mettre à jour l'ordinateur {ordinateur.Id}");
        return NotFound();
    }

    // DELETE /ordinateurs/{id}
    [Authorize(Roles = "Admin")] // seul un admin peut supprimer
    [HttpDelete("{id:int}")]
    public IActionResult Supprimer(int id)
        // Supprime un ordinateur de la liste
    {
        _logger.LogInformation($"Suppression de l'ordinateur {id}");
        var resultat = _service.Supprimer(id);

        if (resultat) return NoContent();
        _logger.LogWarning("Impossible de supprimer l'ordinateur {Id}", id);
        return NotFound();
    }
}