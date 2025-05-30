using System.Runtime.InteropServices.JavaScript;
using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using pnyx.net.util;
    
namespace FidelityOptionsScraper.Scrapers;

public class OptionsScraperService
{
    private readonly BrowserService browserService;

    public OptionsScraperService(BrowserService browserService)
    {
        this.browserService = browserService;
    }

    /// <summary>
    /// Gets call option prices for a given symbol, expiration date, and strike prices
    /// </summary>
    /// <param name="symbol">The stock symbol</param>
    /// <returns>List of OptionData objects with price information</returns>
    public async Task<List<OptionData>?> getCallOptionPrices(string symbol)
    {
        if (browserService.CurrentPage == null)
            throw new InvalidOperationException("Browser not initialized");

        List<OptionData> results = new ();

        try
        {
            // Navigate to the options chain page
            string url = $"https://digital.fidelity.com/ftgw/digital/options-research/option-chain?symbol={symbol}&oarchain=true";
            await browserService.NavigateToAsync(url);

            // Wait for the options chain to load
            await browserService.CurrentPage.WaitForSelectorAsync(".ag-root-wrapper", new PageWaitForSelectorOptions { Timeout = 15000 });

            IReadOnlyList<IElementHandle> dateRowGroup = await browserService.CurrentPage.QuerySelectorAllAsync(".ag-row-group");
            List<(DateTime, int)> dateGroupings = await findDateGroupings(dateRowGroup);
            Console.WriteLine($"DateGroupings count: {dateGroupings.Count}");
            if (dateGroupings.Count == 0)
                return null;

            foreach ((DateTime expirationDate, int rowIndex) in dateGroupings)
            {
                Console.WriteLine();
                Console.WriteLine($"Fetching options for expiration date: {expirationDate.toIso8601Date()}");
                
                IReadOnlyList<IElementHandle> rowElements = await browserService.CurrentPage.QuerySelectorAllAsync(".ag-center-cols-container .ag-row");
                Console.WriteLine($"OptionsElements: {rowElements.Count}");
                if (rowElements.Count == 0)
                    continue;

                List<OptionData> rows = await findOptionData(symbol, expirationDate, rowElements, rowIndex);
                Console.WriteLine($"Rows: {rows.Count}");
                results.AddRange(rows);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting options data for {symbol}: {ex.Message}");
            return null;
        }
        
        if (results.Count == 0)
        {
            Console.WriteLine($"No options data found for {symbol}");
            return null;
        }   
        
        return results;
    }

    private async Task<List<(DateTime, int)>> findDateGroupings(IReadOnlyList<IElementHandle> optionsGroups)
    {
        List<(DateTime, int)> results = new();
        foreach (IElementHandle toCheck in optionsGroups)
        {
            IElementHandle? expirationDateElement = await toCheck.QuerySelectorAsync("span.expiration-date");
            if (expirationDateElement == null)
                continue;

            string? contentText = await expirationDateElement.TextContentAsync();
            if (contentText == null)
                continue;

            DateTime? expirationDate = DateUtil.parseNullable("MMM dd, yyyy", contentText);
            if (expirationDate == null)
                continue;

            string? rowIndexText = await toCheck.GetAttributeAsync("row-index");
            if (rowIndexText == null)
                continue;
            
            int? rowIndex = rowIndexText.parseIntNullable();
            if (rowIndex == null)
                continue;
            
            results.Add((expirationDate.Value, rowIndex.Value));
        }

        return results;
    }

    private async Task<List<OptionData>> findOptionData(string symbol, DateTime expirationDate, IEnumerable<IElementHandle> rows, int headerRowIndex)
    {
        List<OptionData> results = new();
        foreach (IElementHandle toCheck in rows)
        {
            string? rowIndexText = await toCheck.GetAttributeAsync("row-index");
            if (rowIndexText == null)
                continue;
            
            int? rowIndex = rowIndexText.parseIntNullable();
            if (rowIndex == null)
                continue;

            // Filters to strictly the rows for specific date
            if (rowIndex.Value <= headerRowIndex || rowIndex.Value > headerRowIndex + 20)
                continue;
            
            IElementHandle? callLastElement = await toCheck.QuerySelectorAsync("""[col-id="callLast"]""");
            IElementHandle? strikeElement = await toCheck.QuerySelectorAsync("""[col-id="strike"]""");
            if (callLastElement == null || strikeElement == null)
                continue;

            OptionData row = new();
            row.symbol = symbol;
            row.strikePrice = await toDecimal(strikeElement);
            row.expirationDate = expirationDate;
            row.callLastPrice = await toDecimal(callLastElement);
            
            IElementHandle? callAskElement = await toCheck.QuerySelectorAsync("""[col-id="callAsk"]""");
            if (callAskElement != null)
                row.callAskPrice = await toDecimal(callAskElement, "Buy at ");
            
            IElementHandle? callBidElement = await toCheck.QuerySelectorAsync("""[col-id="callBid"]""");
            if (callBidElement != null)
                row.callBidPrice = await toDecimal(callBidElement, "Sell at ");

            Console.WriteLine(row.ToString());
            results.Add(row);
        }
        return results;
    }
    
    private async Task<decimal> toDecimal(IElementHandle element, string? splitAt = null)
    {
        string? contentText = await element.TextContentAsync();
        if (contentText == null)
            throw new InvalidOperationException("Cannot extract text from element: " + element);

        if (splitAt != null)
        {
            Tuple<string, string> parts = ParseExtensions.splitAt(contentText, splitAt);
            if (parts == null)
                throw new InvalidOperationException($"Could not split: {contentText} at {splitAt}");

            contentText = parts.Item2;
        }
        
        return decimal.Parse(contentText);
    }
}