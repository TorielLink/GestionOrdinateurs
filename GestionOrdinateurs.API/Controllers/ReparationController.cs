using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // tout le contrôleur nécessite un token valide
public class ReparationController : ControllerBase
{
    private readonly IReparationService _service;
    private readonly IOrdinateurService _ordinateurService;
    private readonly IStockService _stockService;
    private readonly ILogger<ReparationController> _logger;

    public ReparationController(
        IReparationService service,
        IOrdinateurService ordinateurService,
        IStockService stockService,
        ILogger<ReparationController> logger)
    {
        _service = service;
        _ordinateurService = ordinateurService;
        _stockService = stockService;
        _logger = logger;
    }

    // POST api/reparation
    [Authorize(Roles = "Admin")] // réservé aux admins
    [HttpPost]
    public async Task<IActionResult> SimulerReparation()
    {
        _logger.LogInformation("Simuler reparations");

        // Récupération de la liste d'ordinateurs
        List<Ordinateur> listOrdinateurs = await _ordinateurService.Lister();
        
        if (listOrdinateurs.Count < 5)
                return BadRequest("Il faut minimum cinq ordinateurs pour lancer la simulation.");

        
        string[] techniciens = ["Gaston LaGaffe", "Victor Hugo", "Raiponce"];

        // Une réparation par ordinateur, toutes lancées en parallèle
        List<Task> reparations = listOrdinateurs
            .Select((ordinateur, i) =>
                _service.SimulerReparationAsync(ordinateur, techniciens[i % techniciens.Length]))
            .ToList();
        
        await Task.WhenAll(reparations);

        return Ok($"Les {reparations.Count} réparations sont terminées !");
    }
    
    // GET api/reparation
    [HttpGet]
    public async Task<IActionResult> ListerReparationsAvecInfos()
    {
        _logger.LogInformation("Récupération des réparations avec informations ...");
        return Ok(await _service.ListerReparationsInfos());
    }
    
    // GET api/reparation/stock-ssd
    [HttpGet("stock-ssd")]
    public IActionResult GetStockSSD()
    {
        _logger.LogInformation("Récupération du stock de SSD ...");

        // Lance 10 consommations de SSD en parallèle, une par itération
        Parallel.For(
            0,
            10,
            i => _stockService.ConsommerSSD());

        _logger.LogInformation("Stock de SSD : {StockSSD}", _stockService.StockSSD);

        return Ok(_stockService.StockSSD);
    }
    
    // GET api/reparation/historique
    [HttpGet("historique")]
    public async Task<IActionResult> ObtenirHistorique()
    {
        _logger.LogInformation("Récupération de l'historique des réparations ...");
        return Ok(await _service.ObtenirHistoriqueAsync());
    }
}