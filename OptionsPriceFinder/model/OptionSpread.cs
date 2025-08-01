﻿using System.Transactions;

namespace OptionsPriceFinder.model;

public class OptionSpread : ICloneable
{
    public string symbol { get; set; } = null!;
    public decimal sharePrice { get; set; }

    public DateTime expirationDate { get; set; }
    
    /// <summary>
    /// Ratio of put options to call options, null if not available.
    /// Ratio below 1 is bullish, above 1 is bearish.
    /// </summary>
    public decimal? putCallRatio { get; set; }

    /// <summary>
    /// Beta value of the stock, null if not available.
    /// </summary>
    public decimal? beta { get; set; }
    
    public decimal strikePriceSell { get; set; }
    public decimal optionPriceSell { get; set; }
    public decimal deltaSell { get; set; }
    public decimal strikePriceBuy { get; set; }
    public decimal optionPriceBuy { get; set; }
    public decimal deltaBuy { get; set; }
    
    /// <summary>
    /// Difference between the strike prices of the two options, minus the preminum of the spread.
    /// </summary>
    public decimal maximumLoss { get; set; }
    
    /// <summary>
    /// Difference between costs of the two options, assuming both expire worthless. Premium of the spread.
    /// </summary>
    public decimal maximumGain { get; set; }
    
    /// <summary>
    /// Maximum ratio of loss per gain for the spread. The smaller the ratio, the better the spread.
    /// Ideal would be 1:1, meaning maximum loss equals maximum gain.
    /// </summary>
    public decimal maximumRatio { get; set; }
    
    /// <summary>
    /// Using the delta as a measure of risk, this is probabilist, cost-weighted value of the spread.
    /// </summary>
    public decimal spreadValue { get; set; }
    
    public static OptionSpread fromOptionValues(OptionValues values)
    {
        return new OptionSpread
        {
            symbol = values.symbol,
            sharePrice = values.sharePrice,
            expirationDate = values.expirationDate,
            putCallRatio = values.putCallRatio,
            beta = values.beta,
        };
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public OptionSpread cloneAs()
    {
        return (OptionSpread)MemberwiseClone();
    }
}