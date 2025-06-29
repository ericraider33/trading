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
    
    /// <summary>
    /// Ratio of put options to call options, null if not available.
    /// Ratio below 1 is bullish, above 1 is bearish.
    /// </summary>
    public decimal? putCallRatio { get; set; }

    public override string ToString()
    {
        return $"{symbol}, {expirationDate.toIso8601Date()}, {strikePrice} {callLastPrice:C}, {callBidPrice:C}, {callAskPrice:C}, {putCallRatio:0.00}";
    }
}