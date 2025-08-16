using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using pnyx.net.util;

namespace FidelityOptionsScraper.Scrapers;

public class StockScraperService
{
    private readonly BrowserService browserService;

    public StockScraperService(BrowserService browserService)
    {
        this.browserService = browserService;
    }

    /// <summary>
    /// Gets the current stock price from Fidelity
    /// </summary>
    /// <param name="symbol">The stock symbol</param>
    /// <returns>StockPrice object with current price information</returns>
    public async Task<StockPrice?> getCurrentPrice(string symbol)
    {
        if (browserService.CurrentPage == null)
            throw new InvalidOperationException("Browser not initialized");

        try
        {
            StockPrice result = new StockPrice { Symbol = symbol };

            result.beta = await getBeta(symbol);
            return result;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error getting stock price for {symbol}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Gets the current stock beta from Yahoo Finance
    /// </summary>
    /// <param name="symbol">The stock symbol</param>
    /// <returns>Beta value or null if not available</returns>
    private async Task<decimal?> getBeta(string symbol)
    {
        if (browserService.CurrentPage == null)
            throw new InvalidOperationException("Browser not initialized");

        const string quoteSelector = "div[data-testid=\"quote-statistics\"]";
        try
        {
            // Navigate to the stock quote page
            string url = $"https://finance.yahoo.com/quote/{symbol}/";
            await browserService.NavigateToAsync(url);

            // Wait for the price element to be visible
            await browserService.CurrentPage.WaitForSelectorAsync(quoteSelector, new PageWaitForSelectorOptions { Timeout = 10000 });

            // Extracts quote statistics as text delimited by new lines
            IElementHandle? quoteElement = await browserService.CurrentPage.QuerySelectorAsync(quoteSelector);
            if (quoteElement == null)
                return null;

            string quoteText = await quoteElement.InnerTextAsync();
            List<string> lines = quoteText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int betaIndex = lines.FindIndex(line => line.startsWithIgnoreCase("beta"));
            if (betaIndex < 0 || betaIndex+1 >= lines.Count)
                return null;

            string betaText = lines[betaIndex + 1].Trim();
            return decimal.Parse(betaText);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error getting stock beta for {symbol}: {ex.Message}");
            return null;
        }
    }    
}