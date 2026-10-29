using System.ComponentModel;

namespace GestionOrdinateurs.Shared;

public class Ordinateur(
    int id,
    string nom,
    EtatsOrdinateur etat,
    int stock,
    decimal prix,
    Caracteristiques caracteristiques)
{
    public int Id { get; init; } = id;

    //Les attributs sont des données sensibles ne veut pas qu'un autre classe qu'Ordinateur change la propriété
    //donc je mets les setter en privé
    public string Nom { get; private set; } = nom;
    
    public EtatsOrdinateur Etat { get; private set; } = etat;

    [DefaultValue(0)]
    public int Stock { get; private set; } = stock;
    
    [DefaultValue(0)]
    public decimal Prix {get; private set;} = prix;
    
    public bool Vendu { get; set; } = false;

    public Caracteristiques Caracteristiques { get; private set; } = caracteristiques;
    
    public List<Reparation> Reparations { get; private set; } = new();

    public virtual void Demarrer()
    {
        Console.WriteLine($"{Nom} démarre ! Son état est actuellement : {Etat}");
    }
    
    private Ordinateur() : this(0, string.Empty, default, 0, 0, null!) { }

    protected void PasserEnMaintenance()
    {
        Etat = EtatsOrdinateur.EnMaintenance;
    }
}
