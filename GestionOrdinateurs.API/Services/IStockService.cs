namespace GestionOrdinateurs.API.Services;

public interface IStockService
{
    int StockSSD { get; set; }
    
    void ConsommerSSD();
}