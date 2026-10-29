using System.Threading.Tasks;

namespace GestionOrdinateurs.API.Services;

public class StockService : IStockService
{
    private readonly object _lock = new();
    public int StockSSD { get; set; } = 10;

    public void ConsommerSSD()
    {
        Task.Delay(10).Wait();

        // un seul thread à la fois peut modifier le stock (atomicité)
        lock (_lock)
        {
            StockSSD--;
        }
    }
}