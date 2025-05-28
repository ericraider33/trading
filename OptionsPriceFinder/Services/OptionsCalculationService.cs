using OptionsPriceFinder.Models;
using OptionsPriceFinder.Utils;

namespace OptionsPriceFinder.Services
{
    public class OptionsCalculationService
    {
        private readonly IStockService _stockService;
        private readonly IOptionsService _optionsService;

        public OptionsCalculationService(IStockService stockService, IOptionsService optionsService)
        {
            _stockService = stockService;
            _optionsService = optionsService;
        }

        public async Task<OptionResult> CalculateOptionsForSymbolAsync(string symbol)
        {
            try
            {
                // Get current stock price
                var stockPrice = await _stockService.GetCurrentPriceAsync(symbol);
                
                // Calculate target prices (current + 1%, 2%, 3%)
                decimal price1Percent = Math.Round(stockPrice.CurrentPrice * 1.01m, 2);
                decimal price2Percent = Math.Round(stockPrice.CurrentPrice * 1.02m, 2);
                decimal price3Percent = Math.Round(stockPrice.CurrentPrice * 1.03m, 2);
                
                // Calculate next Friday
                DateTime today = DateTime.Today;
                DateTime nextFriday = DateCalculator.GetNextFriday(today);
                
                // Get options prices for each target price
                var option1Percent = await _optionsService.GetCallOptionPriceAsync(symbol, nextFriday, price1Percent);
                var option2Percent = await _optionsService.GetCallOptionPriceAsync(symbol, nextFriday, price2Percent);
                var option3Percent = await _optionsService.GetCallOptionPriceAsync(symbol, nextFriday, price3Percent);
                
                // Create result
                return new OptionResult
                {
                    Symbol = symbol,
                    CurrentPrice = stockPrice.CurrentPrice,
                    FridayDate = nextFriday.ToString("yyyy-MM-dd"),
                    CallOption1Percent = option1Percent.CallPrice,
                    CallOption2Percent = option2Percent.CallPrice,
                    CallOption3Percent = option3Percent.CallPrice
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating options for {symbol}: {ex.Message}");
                
                // Return partial result with error indication
                return new OptionResult
                {
                    Symbol = symbol,
                    CurrentPrice = 0,
                    FridayDate = DateTime.Today.ToString("yyyy-MM-dd"),
                    CallOption1Percent = null,
                    CallOption2Percent = null,
                    CallOption3Percent = null
                };
            }
        }
    }
}
