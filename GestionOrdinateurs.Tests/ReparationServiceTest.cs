using FluentAssertions;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class ReparationServiceTest
{
    // La fabrique donne toujours des contextes sur la même base en mémoire,
    // sinon la réparation créée par le service ne serait pas visible dans mon contexte de test
    private class FabriqueTest : IDbContextFactory<ParcInformatiqueContext>
    {
        private readonly DbContextOptions<ParcInformatiqueContext> _options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        public ParcInformatiqueContext CreateDbContext()
        {
            return new ParcInformatiqueContext(_options);
        }
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
    public async Task SimulerReparation_UnOrdinateur_DoitCreerReparationTerminee()
    {
        // Mise en place : un ordinateur en base et le service
        FabriqueTest factory = new FabriqueTest();
        await using ParcInformatiqueContext context = factory.CreateDbContext();
        ReparationService service = new ReparationService(context, factory, new JournalService());

        Ordinateur ordinateur = CreerOrdinateur(1, "PC-Test", 999m);
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync();

        // Action : on simule la réparation de l'ordinateur
        await service.SimulerReparationAsync(ordinateur, "Lara Croft");

        // Vérification : il y a une réparation terminée avec le bon technicien
        List<Reparation> reparations = await context.Reparations.ToListAsync();

        reparations.Should().HaveCount(1);
        reparations[0].OrdinateurId.Should().Be(1);
        reparations[0].Technicien.Should().Be("Lara Croft");
        reparations[0].Statut.Should().Be("Terminée");
    }

    [Fact]
    public async Task SimulerReparation_CinqOrdinateurs_DoitCreerCinqReparations()
    {
        // Mise en place : cinq ordinateurs en base
        FabriqueTest factory = new FabriqueTest();
        await using ParcInformatiqueContext context = factory.CreateDbContext();
        ReparationService service = new ReparationService(context, factory, new JournalService());

        List<Ordinateur> ordinateurs = new();
        for (int i = 1; i <= 5; i++)
        {
            ordinateurs.Add(CreerOrdinateur(i, "PC-0" + i, 500m));
        }

        context.Ordinateurs.AddRange(ordinateurs);
        await context.SaveChangesAsync();

        // Action : on lance les cinq réparations en même temps
        List<Task> taches = new();
        foreach (Ordinateur ordinateur in ordinateurs)
        {
            taches.Add(service.SimulerReparationAsync(ordinateur, "Le Roi Arthur"));
        }

        await Task.WhenAll(taches);

        // Vérification : on a bien cinq réparations et elles sont toutes terminées
        List<Reparation> reparations = await context.Reparations.ToListAsync();

        reparations.Should().HaveCount(5);
        reparations.Should().OnlyContain(r => r.Statut == "Terminée");
    }

    [Fact]
    public async Task ObtenirHistorique_ReparationExistante_DoitRetournerLHistorique()
    {
        // Mise en place : un ordinateur et une réparation qui lui est liée
        FabriqueTest factory = new FabriqueTest();
        await using ParcInformatiqueContext context = factory.CreateDbContext();
        ReparationService service = new ReparationService(context, factory, new JournalService());

        context.Ordinateurs.Add(CreerOrdinateur(1, "PC-Test", 999m));
        context.Reparations.Add(new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Maintenance",
            DateDebut = DateTime.Now,
            Statut = "Terminée",
            Technicien = "Lara Croft"
        });
        await context.SaveChangesAsync();

        // Action : on récupère l'historique
        List<HistoriqueReparationRecord> historique = await service.ObtenirHistoriqueAsync();

        // Vérification : le record contient le nom de l'ordinateur et le technicien
        historique.Should().HaveCount(1);
        historique[0].OrdinateurId.Should().Be(1);
        historique[0].NomOrdinateur.Should().Be("PC-Test");
        historique[0].Technicien.Should().Be("Lara Croft");
        historique[0].TypeReparation.Should().Be("Maintenance");
    }
    
    [Fact]
    public async Task Creer_UneReparation_DoitEtreEnregistreeEtLieeAUnOrdinateur()
    {
        // Mise en place : un ordinateur dans la base et le service
        FabriqueTest factory = new FabriqueTest();
        await using ParcInformatiqueContext context = factory.CreateDbContext();
        ReparationService service = new ReparationService(context, factory, new JournalService());

        context.Ordinateurs.Add(CreerOrdinateur(1, "PC-Test", 999m));
        await context.SaveChangesAsync();

        Reparation reparation = new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Maintenance",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = "Esmeralda"
        };

        // Action : on crée la réparation dans la bd
        context.Reparations.Add(reparation);
        await context.SaveChangesAsync();

        // Vérification : la réparation a un Id et on retrouve son ordinateur
        Reparation? resultat = await service.RecupererInfoReparation(reparation.Id);

        resultat.Should().NotBeNull();
        resultat.Id.Should().BeGreaterThan(0); // comme il est généré automatiquement on vérifie juste qu'il est valide
        resultat.Statut.Should().Be("En cours");
        resultat.Technicien.Should().Be("Esmeralda");
        resultat.TypeReparation.Should().Be("Maintenance");
        resultat.UnOrdinateur.Should().NotBeNull();
        resultat.UnOrdinateur.Nom.Should().Be("PC-Test");
    }
}