namespace GestionOrdinateurs.Shared;

public class ParcInitializer
{
    public List<Ordinateur> Ordinateurs { get; set; } = new()
    {
        new Ordinateur(
            1,
            "PC-001",
            EtatsOrdinateur.Disponible,
            5, 1200m,
            new Caracteristiques(
                "Dell",
                16,
                "Intel Core i7",
                TypeOrdinateur.Portable,
                512
            )
        ),

        new Ordinateur(
            2,
            "PC-002",
            EtatsOrdinateur.Disponible,
            5,
            750m,
            new Caracteristiques(
                "HP",
                8,
                "Intel Core i5",
                TypeOrdinateur.Fixe,
                256
            )
        ),

        new Ordinateur(3,
            "PC-003",
            EtatsOrdinateur.EnMaintenance,
            5,
            1500m,
            new Caracteristiques(
                "Lenovo",
                32,
                "AMD Ryzen 7",
                TypeOrdinateur.Serveur,
                1024
            )
        ),

        new Ordinateur(4,
            "PC-004",
            EtatsOrdinateur.HorsService,
            5,
            950m,
            new Caracteristiques(
                "Asus",
                16,
                "AMD Ryzen 5",
                TypeOrdinateur.Portable,
                512
            )
        )
    };
}