namespace GestionOrdinateurs.API.Services;

public class JournalService
{
    public void Ecrire(string message)
    {
        using var writer =
            new StreamWriter(
                "reparations.log",
                append: true);

        writer.WriteLine(message);
    }
}