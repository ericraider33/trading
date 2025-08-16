using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using HtmlAgilityPack;
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
            IElementHandle? root = await browserService.CurrentPage.WaitForSelectorAsync(".ag-root-wrapper", new PageWaitForSelectorOptions { Timeout = 30000 });
            IElementHandle? oarPanelQuote = await browserService.CurrentPage.QuerySelectorAsync(".oar-panel.quote");
            if (root == null || oarPanelQuote == null)
                return null;
            
            string rootHtmlAsText = await root.InnerHTMLAsync();
            string oarPanelQuoteHtmlAsText = await oarPanelQuote.InnerHTMLAsync();
            
            HtmlDocument rootDoc = new HtmlDocument();
            rootDoc.LoadHtml(rootHtmlAsText);            
            HtmlDocument oarPanelQuoteDoc = new HtmlDocument();
            oarPanelQuoteDoc.LoadHtml(oarPanelQuoteHtmlAsText);            
            
            // Get the put/call ratio
            decimal sharePrice = getSharePrice(oarPanelQuoteDoc);
            decimal? putCallRatio = getPutCallRatio(oarPanelQuoteDoc, symbol);
            
            List<(DateTime, int)> dateGroupings = findDateGroupings(rootDoc);
            Console.WriteLine($"DateGroupings count: {dateGroupings.Count}");
            if (dateGroupings.Count == 0)
                return null;

            foreach ((DateTime expirationDate, int rowIndex) in dateGroupings)
            {
                Console.WriteLine();
                Console.WriteLine($"Fetching options for expiration date: {expirationDate.toIso8601Date()}");
                
                // IReadOnlyList<IElementHandle> rowElements = await browserService.CurrentPage.QuerySelectorAllAsync(".ag-center-cols-container .ag-row");
                HtmlNodeCollection rowElements = rootDoc.DocumentNode.SelectNodes("//div[contains(@class, 'ag-center-cols-container')]//div[contains(@class, 'ag-row')]");
                Console.WriteLine($"OptionsElements: {rowElements?.Count ?? 0}");
                if (rowElements == null || rowElements.Count == 0)
                    continue;
                
                List<OptionData> rows = findOptionData(symbol, expirationDate, rowElements, rowIndex);
                Console.WriteLine($"Rows: {rows.Count}");
                results.AddRange(rows);
                
                // Adds put/call ratio to each option
                foreach (OptionData row in rows)
                {
                    row.sharePrice = sharePrice;
                    row.putCallRatio = putCallRatio;
                }
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

    private decimal getSharePrice(HtmlDocument doc)
    {
        HtmlNode quoteContainerElement = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'oar-quote-info-container')]");
        if (quoteContainerElement == null)
            throw new Exception("No quote element found");
        
        HtmlNode quoteElement = quoteContainerElement.SelectSingleNode("//span[@class='oar-quote-last']");
        if (quoteElement == null)
            throw new Exception("No quote element found");
            
        string contentText = quoteElement.InnerText;
        contentText = contentText.Replace("$", "").Trim();
        contentText = contentText.Trim().splitAt(" ").Item1;
        if (String.IsNullOrEmpty(contentText))
            throw new Exception($"Quote text is not formated as expected: {quoteElement.InnerText}");

        return decimal.Parse(contentText);
    }

    private decimal? getPutCallRatio(HtmlDocument doc, string symbol)
    {
        try
        {
            HtmlNode putCallRatioElement = doc.DocumentNode.SelectSingleNode("//div[@class='ratio-value']");
            if (putCallRatioElement == null)
                return null;
            
            string contentText = putCallRatioElement.InnerText;
            contentText = contentText.Trim().splitAt(" ").Item1;
            if (String.IsNullOrEmpty(contentText))
                return null;
            
            return decimal.Parse(contentText);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting put/call ratio for {symbol}: {ex.Message}");
            return null;
        }
    }
    
    private List<(DateTime, int)> findDateGroupings(HtmlDocument doc)
    {
        HtmlNodeCollection optionsGroups = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ag-row-group')]");
        
        List<(DateTime, int)> results = new();
        if (optionsGroups == null)
            return results;
        
        foreach (HtmlNode toCheck in optionsGroups)
        {
            HtmlNode expirationDateElement = toCheck.SelectSingleNode("//span[@class='expiration-date']");
            if (expirationDateElement == null)
                continue;

            string? contentText = expirationDateElement.InnerText;
            if (contentText == null)
                continue;
            
            // Removes any formatting like "(Weekly)" or "(Monthly)"
            contentText = contentText.splitAt("(").Item1.Trim();
            
            DateTime? expirationDate = DateUtil.parseNullable("MMM dd, yyyy", contentText);
            if (expirationDate == null)
                continue;

            string rowIndexText = toCheck.GetAttributeValue("row-index", "-1");
            if (rowIndexText == "-1")
                continue;
            
            int? rowIndex = rowIndexText.parseIntNullable();
            if (rowIndex == null)
                continue;
            
            results.Add((expirationDate.Value, rowIndex.Value));
        }

        return results;
    }

    private List<OptionData> findOptionData(string symbol, DateTime expirationDate, HtmlNodeCollection rows, int headerRowIndex)
    {
        List<OptionData> results = new();
        foreach (HtmlNode toCheck in rows)
        {
            string rowIndexText = toCheck.GetAttributeValue("row-index", "-1");
            if (rowIndexText == "-1")
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

            HtmlNode strikeElement = toCheck.SelectSingleNode("./div[@col-id='strike']");
            if (strikeElement == null)
                continue;
            row.strikePrice = toDecimal(strikeElement);

            if (!populateCallOptionData(row, toCheck))
                continue;

            if (!populatePutOptionData(row, toCheck))
                continue;

            results.Add(row);
        }
        return results;
    }
    
    private bool populateCallOptionData(OptionData row, HtmlNode toCheck)
    {
        HtmlNode callLastElement = toCheck.SelectSingleNode("./div[@col-id='callLast']");
        if (callLastElement == null)
            return false;

        row.callLastPrice = toDecimal(callLastElement);
            
        HtmlNode callAskElement = toCheck.SelectSingleNode("./div[@col-id='callAsk']");
        if (callAskElement != null)
            row.callAskPrice = toDecimal(callAskElement, "Buy at ");
            
        HtmlNode callBidElement = toCheck.SelectSingleNode("./div[@col-id='callBid']");
        if (callBidElement != null)
            row.callBidPrice = toDecimal(callBidElement, "Sell at ");
        
        HtmlNode callOpenInterestElement = toCheck.SelectSingleNode("./div[@col-id='callOpenInterest']");
        if (callOpenInterestElement != null)
            row.callOpenInterest = toDecimal(callOpenInterestElement);

        HtmlNode callImpliedVolatilityElement = toCheck.SelectSingleNode("./div[@col-id='callImpliedVolatility']");
        if (callImpliedVolatilityElement != null)
            row.callImpliedVolatility = toDecimalPercentage(callImpliedVolatilityElement);

        HtmlNode callDeltaElement = toCheck.SelectSingleNode("./div[@col-id='callDelta']");
        if (callDeltaElement != null)
            row.callDelta = toDecimalNullable(callDeltaElement);
        
        return true;
    }
    
    private bool populatePutOptionData(OptionData row, HtmlNode toCheck)
    {
        HtmlNode putLastElement = toCheck.SelectSingleNode("./div[@col-id='putLast']");
        if (putLastElement == null)
            return false;

        row.putLastPrice = toDecimal(putLastElement);
            
        HtmlNode putAskElement = toCheck.SelectSingleNode("./div[@col-id='putAsk']");
        if (putAskElement != null)
            row.putAskPrice = toDecimal(putAskElement, "Buy at ");
            
        HtmlNode putBidElement = toCheck.SelectSingleNode("./div[@col-id='putBid']");
        if (putBidElement != null)
            row.putBidPrice = toDecimal(putBidElement, "Sell at ");

        HtmlNode putOpenInterestElement = toCheck.SelectSingleNode("./div[@col-id='putOpenInterest']");
        if (putOpenInterestElement != null)
            row.putOpenInterest = toDecimal(putOpenInterestElement);

        HtmlNode putImpliedVolatilityElement = toCheck.SelectSingleNode("./div[@col-id='putImpliedVolatility']");
        if (putImpliedVolatilityElement != null)
            row.putImpliedVolatility = toDecimalPercentage(putImpliedVolatilityElement);

        HtmlNode putDeltaElement = toCheck.SelectSingleNode("./div[@col-id='putDelta']");
        if (putDeltaElement != null)
            row.putDelta = toDecimalNullable(putDeltaElement);
        
        return true;
    }
    
    private decimal toDecimal(HtmlNode element, string? splitAt = null)
    {
        string? contentText = element.InnerText;
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

    private decimal? toDecimalPercentage(HtmlNode element)
    {
        return toDecimalNullable(element, s => TextUtil.removeEnding(s, "%"));
    }
    
    private decimal? toDecimalNullable(HtmlNode element, Func<string,string?>? cleanup = null)
    {
        string? contentText = element.InnerText;
        if (contentText == null)
            throw new InvalidOperationException("Cannot extract text from element: " + element);

        if (cleanup != null)
            contentText = cleanup(contentText);
        
        if (String.IsNullOrWhiteSpace(contentText))
            return null;
        
        if (contentText == "--" || contentText == "-")
            return null;
        
        return decimal.Parse(contentText);
    }
}