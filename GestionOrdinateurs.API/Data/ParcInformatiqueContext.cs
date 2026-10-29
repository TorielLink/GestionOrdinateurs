using Microsoft.EntityFrameworkCore;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Data;
public class ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : DbContext(options)
{
    public DbSet<Ordinateur> Ordinateurs { get; set; }
    public DbSet<Reparation> Reparations { get; set; }
    public DbSet<Utilisateur> Utilisateurs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clef primaire d'Ordinateur
        modelBuilder.Entity<Ordinateur>().HasKey(o => o.Id);

        // Clef primaire de Reparation
        modelBuilder.Entity<Reparation>().HasKey(r => r.Id);

        // Caracteristiques est stockée dans la table Ordinateurs
        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(o => o.Caracteristiques);

        // Un ordinateur a plusieurs réparations
        modelBuilder.Entity<Ordinateur>()
            .HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur)
            .HasForeignKey(r => r.OrdinateurId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}