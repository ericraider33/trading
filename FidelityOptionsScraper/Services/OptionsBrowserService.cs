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
                
            Console.WriteLine($"Current beta for {symbol}: ${stockPrice?.beta}");
            List<OptionData>? options = await optionsScraper.getCallAndPutOptionPrices(symbol);
            if (options == null)
                return null;

            // Saves the current stock price to each option
            foreach (OptionData option in options)
            {
                option.beta = stockPrice?.beta;
            }

            return options;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error finding options for {symbol}: {ex.Message}");
            await Console.Error.WriteLineAsync(ex.StackTrace);
            return null;
        }
    }
}