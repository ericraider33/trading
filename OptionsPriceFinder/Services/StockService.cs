using OptionsPriceFinder.Models;

namespace OptionsPriceFinder.Services
{
    public interface IStockService
    {
        Task<StockPrice> GetCurrentPriceAsync(string symbol);
    }

    public class StockService : IStockService
    {
        // This will be implemented with Polygon.io API integration
        public async Task<StockPrice> GetCurrentPriceAsync(string symbol)
        {
            // Placeholder implementation until API integration
            return await Task.FromResult(new StockPrice
            {
                Symbol = symbol,
                CurrentPrice = 0, // Will be populated from API
                RetrievalTime = DateTime.UtcNow
            });
        }
    }
}
