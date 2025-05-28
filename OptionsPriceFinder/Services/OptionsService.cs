using OptionsPriceFinder.Models;

namespace OptionsPriceFinder.Services;

public interface IOptionsService
{
    Task<OptionData> getCallOptionPrice(string symbol, DateTime expirationDate, decimal strikePrice);
}

public class OptionsService : IOptionsService
{
    private readonly PolygonApiClient polygonClient;

    public OptionsService(PolygonApiClient polygonClient)
    {
        this.polygonClient = polygonClient;
    }

    public async Task<OptionData> getCallOptionPrice(string symbol, DateTime expirationDate, decimal strikePrice)
    {
        var options = await polygonClient.GetOptionsDataAsync(symbol, expirationDate, strikePrice);
            
        // Find the closest match to the target strike price
        var closestOption = options
            .OrderBy(o => Math.Abs(o.StrikePrice - strikePrice))
            .FirstOrDefault();

        return closestOption ?? new OptionData
        {
            Symbol = symbol,
            ExpirationDate = expirationDate,
            StrikePrice = strikePrice,
            CallPrice = null
        };
    }

}