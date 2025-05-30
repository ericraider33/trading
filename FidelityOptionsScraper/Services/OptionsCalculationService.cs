using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Scrapers;
using FidelityOptionsScraper.Utils;

namespace FidelityOptionsScraper;

public class OptionsCalculationService
{
    private readonly StockScraperService stockScraper;
    private readonly OptionsScraperService optionsScraper;

    public OptionsCalculationService(StockScraperService stockScraper, OptionsScraperService optionsScraper)
    {
        this.stockScraper = stockScraper;
        this.optionsScraper = optionsScraper;
    }

    /// <summary>
    /// Calculates options data for a given symbol
    /// </summary>
    /// <param name="symbol">The stock symbol</param>
    /// <returns>OptionResult with all required data</returns>
    public async Task<OptionResult?> calculateOptionsForSymbol(string symbol)
    {
        try
        {
            Console.WriteLine($"Processing {symbol}...");
                
            // Get current stock price
            StockPrice? stockPrice = await stockScraper.getCurrentPrice(symbol);
            if (stockPrice == null)
                return null;
                
            Console.WriteLine($"Current price for {symbol}: ${stockPrice.CurrentPrice}");
            if (stockPrice.CurrentPrice <= 0)
            {
                Console.WriteLine($"Warning: Invalid price for {symbol}. Skipping options calculation.");
                return null;
            }
                
            // Calculate target prices (current + 1%, 2%, 3%)
            decimal price1Percent = Math.Round(stockPrice.CurrentPrice * 1.01m, 2);
            decimal price2Percent = Math.Round(stockPrice.CurrentPrice * 1.02m, 2);
            decimal price3Percent = Math.Round(stockPrice.CurrentPrice * 1.03m, 2);
                
            Console.WriteLine($"Target prices: +1%=${price1Percent}, +2%=${price2Percent}, +3%=${price3Percent}");
                
            // Calculate next Friday
            DateTime today = DateTime.Today;
            DateTime nextFriday = DateCalculator.GetNextFriday(today);
            string fridayDate = nextFriday.ToString("yyyy-MM-dd");
                
            Console.WriteLine($"Next Friday: {fridayDate}");
                
            // Get options prices for each target price
            var targetPrices = new List<decimal> { price1Percent, price2Percent, price3Percent };
            var optionsData = await optionsScraper.getCallOptionPrices(symbol, nextFriday);
                
            // Find the option prices for each target
            decimal? callOption1Percent = null;
            decimal? callOption2Percent = null;
            decimal? callOption3Percent = null;
                
            foreach (var option in optionsData)
            {
                if (Math.Abs(option.strikePrice - price1Percent) < 0.01m)
                {
                    callOption1Percent = option.callBidPrice;
                }
                else if (Math.Abs(option.strikePrice - price2Percent) < 0.01m)
                {
                    callOption2Percent = option.callBidPrice;
                }
                else if (Math.Abs(option.strikePrice - price3Percent) < 0.01m)
                {
                    callOption3Percent = option.callBidPrice;
                }
            }
                
            Console.WriteLine($"Call option prices: +1%=${callOption1Percent}, +2%=${callOption2Percent}, +3%=${callOption3Percent}");
                
            // Create result
            return new OptionResult
            {
                Symbol = symbol,
                CurrentPrice = stockPrice.CurrentPrice,
                FridayDate = fridayDate,
                CallOption1Percent = callOption1Percent,
                CallOption2Percent = callOption2Percent,
                CallOption3Percent = callOption3Percent
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