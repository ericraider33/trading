using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Scrapers;

namespace FidelityOptionsScraper.Services;

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
    /// Fetches raw options data for a given symbol
    /// </summary>
    /// <param name="symbol">The stock symbol</param>
    public async Task<List<OptionData>?> fetchOptionsForSymbol(string symbol)
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

            List<OptionData>? options = await optionsScraper.getCallOptionPrices(symbol);
            if (options == null)
                return null;

            // Saves the current stock price to each option
            foreach (OptionData option in options)
                option.sharePrice = stockPrice.CurrentPrice;

            return options;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding options for {symbol}: {ex.Message}");
            return null;
        }
    }
}