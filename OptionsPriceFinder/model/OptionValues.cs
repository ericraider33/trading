namespace OptionsPriceFinder.model;

public class OptionValues
{
    public string symbol { get; set; } = null!;
    public decimal sharePrice { get; set; }

    public DateTime expirationDate { get; set; }
    public int shares { get; set; }
    public int options { get; set; }
    public decimal costBasis { get; set; }
    
    /// <summary>
    /// Ratio of put options to call options, null if not available.
    /// Ratio below 1 is bullish, above 1 is bearish.
    /// </summary>
    public decimal? putCallRatio { get; set; }
    
    public decimal? strikePrice1 { get; set; }
    public decimal? strikePrice2 { get; set; }
    public decimal? strikePrice3 { get; set; }
    
    public decimal? callPrice1 { get; set;}
    public decimal? callPrice2 { get; set; }
    public decimal? callPrice3 { get; set; }
    
    public decimal? incomePercent1 { get; set; }
    public decimal? incomePercent2 { get; set; }
    public decimal? incomePercent3 { get; set; }

    public override string ToString()
    {
        return $"{symbol},{sharePrice},{incomePercent1},{incomePercent2},{incomePercent3}";
    }
}