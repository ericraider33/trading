using OptionsPriceFinder.Models;

namespace OptionsPriceFinder.Services
{
    public interface IOptionsService
    {
        Task<OptionData> GetCallOptionPriceAsync(string symbol, DateTime expirationDate, decimal strikePrice);
    }

    public class OptionsService : IOptionsService
    {
        // This will be implemented with Polygon.io API integration
        public async Task<OptionData> GetCallOptionPriceAsync(string symbol, DateTime expirationDate, decimal strikePrice)
        {
            // Placeholder implementation until API integration
            return await Task.FromResult(new OptionData
            {
                Symbol = symbol,
                ExpirationDate = expirationDate,
                StrikePrice = strikePrice,
                CallPrice = null // Will be populated from API
            });
        }
    }
}
