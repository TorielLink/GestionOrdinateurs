namespace GestionOrdinateurs.Shared;

public class Caracteristiques
{
    public string Marque { get; private set; }
    
    public int RamGo { get; private set; }
    
    public string Processeur { get; private set; }
    
    public TypeOrdinateur TypeOrdi { get; set; }
    
    public int StockageGo { get; set; }

    public Caracteristiques(string marque, int ramGo, string processeur, TypeOrdinateur typeOrdi, int stockageGo)
    {
        Marque = marque;
        RamGo = ramGo;
        Processeur = processeur;
        TypeOrdi = typeOrdi;
        StockageGo = stockageGo;
    }
}