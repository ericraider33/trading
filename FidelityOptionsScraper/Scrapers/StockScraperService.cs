using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;

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

        const string quoteSelector = ".nre-quick-quote-price";
        try
        {
            // Navigate to the stock quote page
            string url = $"https://digital.fidelity.com/prgw/digital/research/quote/dashboard/summary?symbol={symbol}";
            await browserService.NavigateToAsync(url);

            // Wait for the price element to be visible
            await browserService.CurrentPage.WaitForSelectorAsync(quoteSelector, new PageWaitForSelectorOptions { Timeout = 10000 });

            // Extract the current price
            string priceText = await browserService.CurrentPage.TextContentAsync(quoteSelector) ?? "0";
            priceText = priceText.Trim().Replace("$", "").Replace(",", "");

            if (decimal.TryParse(priceText, out decimal price))
            {
                return new StockPrice
                {
                    Symbol = symbol,
                    CurrentPrice = price,
                    RetrievalTime = DateTime.Now
                };
            }

            Console.WriteLine($"Failed to parse price: {priceText}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting stock price for {symbol}: {ex.Message}");
            return null;
        }
    }
}