namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    private readonly List<Ordinateur> _ordinateurs = [];

    public void Ajouter(Ordinateur ordinateur)
    {
        _ordinateurs.Add(ordinateur);
    }

    public Ordinateur? this[int ordinateurId]
    // Recherche par id
    {
        get
        {
            foreach (Ordinateur ordinateur in _ordinateurs)
            {
                if (ordinateur.Id == ordinateurId)
                    return ordinateur;
            }

            return null; // pas trouvé
        }
    }

    public Ordinateur? this[string reference]
    // Recherche par nom
    {
        get
        {
            foreach (Ordinateur ordinateur in _ordinateurs)
            {
                if (ordinateur.Nom == reference)
                    return ordinateur;
            }

            return null; // pas trouvé
        }
    }
}