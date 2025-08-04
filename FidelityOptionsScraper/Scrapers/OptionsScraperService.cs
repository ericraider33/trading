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
    public async Task<List<OptionData>?> getCallAndPutOptionPrices(string symbol)
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
            await browserService.CurrentPage.WaitForSelectorAsync(".ag-root-wrapper", new PageWaitForSelectorOptions { Timeout = 30000 });

            // Get the put/call ratio
            decimal? putCallRatio = await getPutCallRatio(symbol);
            
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
                
                // Adds put/call ratio to each option
                foreach (OptionData row in rows)
                    row.putCallRatio = putCallRatio;
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error getting options data for {symbol}: {ex.Message}");
            return null;
        }
        
        if (results.Count == 0)
        {
            Console.WriteLine($"No options data found for {symbol}");
            return null;
        }   
        
        return results;
    }
    
    private async Task<decimal?> getPutCallRatio(string symbol)
    {
        if (browserService.CurrentPage == null)
            throw new InvalidOperationException("Browser not initialized");

        try
        {
            IElementHandle? putCallRatioElement = await browserService.CurrentPage.QuerySelectorAsync(".ratio-value");
            if (putCallRatioElement == null)
                return null;

            string? contentText = await putCallRatioElement.TextContentAsync();
            if (contentText == null)
                return null;

            contentText = contentText.Trim().splitAt(" ").Item1;
            if (String.IsNullOrEmpty(contentText))
                return null;
            
            return decimal.Parse(contentText);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error getting put/call ratio for {symbol}: {ex.Message}");
            return null;
        }
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
            
            // Removes any formatting like "(Weekly)" or "(Monthly)"
            contentText = contentText.splitAt("(").Item1.Trim();
            
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
            

            OptionData row = new();
            row.symbol = symbol;
            row.expirationDate = expirationDate;

            IElementHandle? strikeElement = await toCheck.QuerySelectorAsync("""[col-id="strike"]""");
            if (strikeElement == null)
                continue;
            row.strikePrice = await toDecimal(strikeElement);

            if (!await populateCallOptionData(row, toCheck))
                continue;

            if (!await populatePutOptionData(row, toCheck))
                continue;

            results.Add(row);
        }
        return results;
    }
    
    private async Task<bool> populateCallOptionData(OptionData row, IElementHandle toCheck)
    {
        IElementHandle? callLastElement = await toCheck.QuerySelectorAsync("""[col-id="callLast"]""");
        if (callLastElement == null)
            return false;

        row.callLastPrice = await toDecimal(callLastElement);
            
        IElementHandle? callAskElement = await toCheck.QuerySelectorAsync("""[col-id="callAsk"]""");
        if (callAskElement != null)
            row.callAskPrice = await toDecimal(callAskElement, "Buy at ");
            
        IElementHandle? callBidElement = await toCheck.QuerySelectorAsync("""[col-id="callBid"]""");
        if (callBidElement != null)
            row.callBidPrice = await toDecimal(callBidElement, "Sell at ");
        
        IElementHandle? callOpenInterestElement = await toCheck.QuerySelectorAsync("""[col-id="callOpenInterest"]""");
        if (callOpenInterestElement != null)
            row.callOpenInterest = await toDecimal(callOpenInterestElement);

        IElementHandle? callImpliedVolatilityElement = await toCheck.QuerySelectorAsync("""[col-id="callImpliedVolatility"]""");
        if (callImpliedVolatilityElement != null)
            row.callImpliedVolatility = await toDecimalPercentage(callImpliedVolatilityElement);

        IElementHandle? callDeltaElement = await toCheck.QuerySelectorAsync("""[col-id="callDelta"]""");
        if (callDeltaElement != null)
            row.callDelta = await toDecimal(callDeltaElement);
        
        return true;
    }
    
    private async Task<bool> populatePutOptionData(OptionData row, IElementHandle toCheck)
    {
        IElementHandle? putLastElement = await toCheck.QuerySelectorAsync("""[col-id="putLast"]""");
        if (putLastElement == null)
            return false;

        row.putLastPrice = await toDecimal(putLastElement);
            
        IElementHandle? putAskElement = await toCheck.QuerySelectorAsync("""[col-id="putAsk"]""");
        if (putAskElement != null)
            row.putAskPrice = await toDecimal(putAskElement, "Buy at ");
            
        IElementHandle? putBidElement = await toCheck.QuerySelectorAsync("""[col-id="putBid"]""");
        if (putBidElement != null)
            row.putBidPrice = await toDecimal(putBidElement, "Sell at ");

        IElementHandle? putOpenInterestElement = await toCheck.QuerySelectorAsync("""[col-id="putOpenInterest"]""");
        if (putOpenInterestElement != null)
            row.putOpenInterest = await toDecimal(putOpenInterestElement);

        IElementHandle? putImpliedVolatilityElement = await toCheck.QuerySelectorAsync("""[col-id="putImpliedVolatility"]""");
        if (putImpliedVolatilityElement != null)
            row.putImpliedVolatility = await toDecimalPercentage(putImpliedVolatilityElement);

        IElementHandle? putDeltaElement = await toCheck.QuerySelectorAsync("""[col-id="putDelta"]""");
        if (putDeltaElement != null)
            row.putDelta = await toDecimal(putDeltaElement);
        
        return true;
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

    private Task<decimal?> toDecimalPercentage(IElementHandle element)
    {
        return toDecimalNullable(element, s => TextUtil.removeEnding(s, "%"));
    }
    
    private async Task<decimal?> toDecimalNullable(IElementHandle element, Func<string,string?> cleanup)
    {
        string? contentText = await element.TextContentAsync();
        if (contentText == null)
            throw new InvalidOperationException("Cannot extract text from element: " + element);

        contentText = cleanup(contentText);
        if (String.IsNullOrWhiteSpace(contentText))
            return null;
        
        if (contentText == "--" || contentText == "-")
            return null;
        
        return decimal.Parse(contentText);
    }
}