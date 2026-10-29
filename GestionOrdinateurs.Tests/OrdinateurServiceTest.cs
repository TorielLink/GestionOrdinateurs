using FluentAssertions;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    private static Ordinateur CreerOrdinateur(int id, string nom, decimal prix) =>
        new Ordinateur(
            id,
            nom,
            EtatsOrdinateur.Disponible,
            5,
            prix,
            new Caracteristiques("Dell", 16, "Intel Core i7", TypeOrdinateur.Portable, 512
            )
        );

    [Fact]
    public async Task Ajouter_UnOrdinateur_DoitAjouterOrdinateur()
    {
        // Mise en place : on doit avoir la base de donnée des ordinateurs et en ajouter un nouveau
        await using ParcInformatiqueContext context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context, new JournalService());
        Ordinateur ordinateur = CreerOrdinateur(10, "PC-Test", 1999m);

        // Action à tester : on ajoute le nouvel ordinateur à la liste
        service.Ajouter(ordinateur);

        // Tester les résultats : on vérifie que l'ordinateur a été ajouté
        Ordinateur? resultat = await service.ObtenirParId(10);

        resultat.Should().NotBeNull();
        resultat.Nom.Should().Be("PC-Test");
        resultat.Caracteristiques.Marque.Should().Be("Dell");
    }

    [Fact]
    public async Task Supprimer_OrdinateurExistant_DoitLeSupprimer()
    {
        // Mise en place : il nous faut un ordinateur au minimum dans la base de données
        await using ParcInformatiqueContext context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context, new JournalService());
        Ordinateur ordinateur = CreerOrdinateur(10, "PC-Test", 760m);

        service.Ajouter(ordinateur);

        // Vérification que l'ordinateur existe bien avant la suppression
        Assert.NotNull(await service.ObtenirParId(10));

        // Action : on supprime l'ordinateur 
        bool resultat = service.Supprimer(10);

        // Tester résultats : l'ordinateur a-t-il bien disparu de la liste ?
        resultat.Should().BeTrue();
        Ordinateur? ordinateurApres = await service.ObtenirParId(10);
        ordinateurApres.Should().BeNull();
    }

    [Fact]
    public void Supprimer_OrdinateurInexistant_DoitRetournerFalse()
    {
        // Mise en place : notre base de données
        using ParcInformatiqueContext context = CreerContexte();
        OrdinateurService service = new OrdinateurService(context, new JournalService());

        // Action : on essaye de supprimer un ordinateur qui ne fait pas partie de la liste
        bool resultat = service.Supprimer(999);

        // Vérification : est-ce que la suppression a bien échouée ?
        resultat.Should().BeFalse();
    }
}