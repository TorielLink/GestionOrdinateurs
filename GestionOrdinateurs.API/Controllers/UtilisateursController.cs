using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UtilisateursController : ControllerBase
{
    private readonly IUtilisateurService _service;
    private readonly ILogger<UtilisateursController> _logger;

    public UtilisateursController(
        IUtilisateurService service,
        ILogger<UtilisateursController> logger
        )
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/utilisateurs/{login}
    [HttpGet("{login}")]
    public async Task<IActionResult> ChercherUtilisateurParLogin(string login)
    {
        _logger.LogInformation("Récupération de l'utilisateur {Login}...", login);
        Utilisateur? utilisateur = await _service.ObtenirParLogin(login);

        if (utilisateur != null) return Ok(utilisateur);
        
        _logger.LogWarning("Utilisateur {Login} introuvable", login);
        return NotFound();
    }

    // GET /api/utilisateurs/
    [HttpGet]
    public async Task<IActionResult> RecupererTousLesUtilisateurs()
    {
        _logger.LogInformation("Récupération de la liste des utilisateurs...");
        return Ok(await _service.ListerUtilisateurs());
    }
}