using pnyx.net.util;

namespace FidelityOptionsScraper.Models;

public class OptionData
{
    public string symbol { get; set; } = null!;
    public decimal sharePrice { get; set; }
    public decimal strikePrice { get; set; }
    public DateTime expirationDate { get; set; }

    public decimal callLastPrice { get; set; }
    public decimal? callBidPrice { get; set; }
    public decimal? callAskPrice { get; set; }
    public decimal? callOpenInterest { get; set; }
    public decimal? callImpliedVolatility { get; set; }
    public decimal? callDelta { get; set; }

    public decimal putLastPrice { get; set; }
    public decimal? putBidPrice { get; set; }
    public decimal? putAskPrice { get; set; }
    public decimal? putOpenInterest { get; set; }
    public decimal? putImpliedVolatility { get; set; }
    public decimal? putDelta { get; set; }
    
    /// <summary>
    /// Ratio of put options to call options, null if not available.
    /// Ratio below 1 is bullish, above 1 is bearish.
    /// </summary>
    public decimal? putCallRatio { get; set; }

    /// <summary>
    /// Beta value of the stock, null if not available.
    /// </summary>
    public decimal? beta { get; set; }

    public override string ToString()
    {
        return $"{symbol}, {expirationDate.toIso8601Date()}, {strikePrice} " +
               $"{callLastPrice:C}, {callBidPrice:C}, {callAskPrice:C}, " +
               $"{putLastPrice:C}, {putBidPrice:C}, {putAskPrice:C}, " +
               $"{putCallRatio:0.00}";
    }
    
    public decimal? putMidPointPrice
    {
        get
        {
            if (!putBidPrice.HasValue || !putAskPrice.HasValue) 
                return null;
            
            if (putBidPrice.Value == 0 || putAskPrice.Value == 0)
                return null;
            
            return (putBidPrice.Value + putAskPrice.Value) / 2;
        }
    }
    
    public decimal? callMidPointPrice
    {
        get
        {
            if (!callBidPrice.HasValue || !callAskPrice.HasValue) 
                return null;
            
            return (callBidPrice.Value + callAskPrice.Value) / 2;
        }
    }
    
    public decimal? getPutPriceBestGuess()
    {
        // missing bid or ask price
        if (!putBidPrice.HasValue || !putAskPrice.HasValue) 
            return null;
        
        // missing bid or ask price
        if (putBidPrice.Value == 0 || putAskPrice.Value == 0)
            return null;

        // none sense range, cannot calculate best guess
        if (putBidPrice.Value >= putAskPrice.Value)
            return null;                

        // missing last price
        if (putLastPrice == 0)
            return null;

        decimal midPoint = (putBidPrice.Value + putAskPrice.Value) / 2;
        decimal weightedLow = putBidPrice.Value * 0.75m + putAskPrice.Value * 0.25m;
        decimal weightedHigh = putBidPrice.Value * 0.25m + putAskPrice.Value * 0.75m;
        
        // Checks if the last price is well within the bid-ask range,
        // in which case it is returned as the best guess.
        if (putLastPrice >= weightedLow && putLastPrice <= weightedHigh)
            return putLastPrice;
        
        if (putLastPrice > putBidPrice.Value && putLastPrice < weightedLow)
            return putLastPrice;
            
        // If the last price is below the bid price, return a weighted average that favors the bid price.
        if (putLastPrice <= putBidPrice.Value)
            return putBidPrice.Value * 0.90m + putAskPrice.Value * 0.10m;
        
        // When higher, simply return midpoint so-as to avoid overestimating the price. 
        return midPoint;
    }
    
    public decimal? getCallPriceBestGuess()
    {
        // missing bid or ask price
        if (!callBidPrice.HasValue || !callAskPrice.HasValue) 
            return null;
        
        // missing bid or ask price
        if (callBidPrice.Value == 0 || callAskPrice.Value == 0)
            return null;

        // none sense range, cannot calculate best guess
        if (callBidPrice.Value >= callAskPrice.Value)
            return null;                

        // missing last price
        if (callLastPrice == 0)
            return null;

        decimal midPoint = (callBidPrice.Value + callAskPrice.Value) / 2;
        decimal weightedLow = callBidPrice.Value * 0.75m + callAskPrice.Value * 0.25m;
        decimal weightedHigh = callBidPrice.Value * 0.25m + callAskPrice.Value * 0.75m;
        
        // Checks if the last price is well within the bid-ask range,
        // in which case it is returned as the best guess.
        if (callLastPrice >= weightedLow && callLastPrice <= weightedHigh)
            return callLastPrice;
        
        if (callLastPrice <= callAskPrice.Value && callLastPrice >= weightedHigh)
            return callLastPrice;
            
        // If the last price is above the ask price, return a weighted average that favors the ask price.
        if (callLastPrice >= callAskPrice.Value)
            return callAskPrice.Value * 0.90m + callBidPrice.Value * 0.10m;
        
        // When lower, simply return midpoint so-as to avoid underestimating the price. 
        return midPoint;
    }
    
}