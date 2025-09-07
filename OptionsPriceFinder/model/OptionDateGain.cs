namespace OptionsPriceFinder.model;

/// <summary>
/// Finds a strike price that is common across dates and calculates the gain between dates.
/// Strike price is selected by finding the strike price closest to the current share price plus/minus 3%.
/// </summary>
public class OptionDateGain
{
    public string symbol { get; set; } = null!;
    public decimal sharePrice { get; set; }
    
    /// <summary>
    /// Ratio of put options to call options, null if not available.
    /// Ratio below 1 is bullish, above 1 is bearish.
    /// </summary>
    public decimal? putCallRatio { get; set; }

    /// <summary>
    /// Beta value of the stock, null if not available.
    /// </summary>
    public decimal? beta { get; set; }

    public decimal strikePrice { get; set; }
    
    public class InfoForDate
    {
        public DateTime expirationDate { get; set; }
        public decimal? optionPrice { get; set;}
        public decimal? incomePercent { get; set; }
    }

    public List<InfoForDate> Values { get; } = new();
}