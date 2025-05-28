using OptionsPriceFinder.Models;

namespace OptionsPriceFinder.Services;

public interface IStockService
{
    Task<StockPrice> getCurrentPrice(string symbol);
}

public class StockService : IStockService
{
    private readonly PolygonApiClient polygonClient;

    public StockService(PolygonApiClient polygonClient)
    {
        this.polygonClient = polygonClient;
    }

    public async Task<StockPrice> getCurrentPrice(string symbol)
    {
        return await polygonClient.GetCurrentStockPriceAsync(symbol);
    }

}