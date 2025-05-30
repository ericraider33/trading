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
    /// <param name="expirationDate">The option expiration date</param>
    /// <param name="strikePrices">List of strike prices to find</param>
    /// <returns>List of OptionData objects with price information</returns>
    public async Task<List<OptionData>?> getCallOptionPrices(string symbol, DateTime expirationDate)
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
            int? rowIndex = await findDateGroupings(dateRowGroup, expirationDate);
            Console.WriteLine($"DateGroupings rowIndex: {rowIndex}");
            if (rowIndex == null)
                return results;

            IReadOnlyList<IElementHandle> rowElements = await browserService.CurrentPage.QuerySelectorAllAsync(".ag-center-cols-container .ag-row");
            Console.WriteLine($"OptionsElements: {rowElements.Count}");
            if (rowElements.Count == 0)
                return results;

            List<OptionData> rows = await findOptionData(symbol, expirationDate, rowElements, rowIndex.Value);
            Console.WriteLine($"Rows: {rows.Count}");
            results.AddRange(rows);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting options data for {symbol}: {ex.Message}");
            return null;
        }
        
        return results;
    }

    private async Task<int?> findDateGroupings(IReadOnlyList<IElementHandle> optionsGroups, DateTime expirationDate)
    {
        string formattedDate = expirationDate.ToString("MMM dd, yyyy");
        
        foreach (IElementHandle toCheck in optionsGroups)
        {
            IElementHandle? expirationDateElement = await toCheck.QuerySelectorAsync("span.expiration-date");
            if (expirationDateElement == null)
                continue;

            string? contentText = await expirationDateElement.TextContentAsync();
            if (contentText == null)
                continue;

            if (!contentText.startsWithIgnoreCase(formattedDate))
                continue;

            string? rowIndexText = await toCheck.GetAttributeAsync("row-index");
            if (rowIndexText == null)
                continue;
            
            int? rowIndex = rowIndexText.parseIntNullable();
            if (rowIndex != null)
                return rowIndex;
        }

        return null;
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