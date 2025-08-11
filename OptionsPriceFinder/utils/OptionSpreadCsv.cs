using Microsoft.Extensions.Logging;
using OptionsPriceFinder.model;
using trading.util;

namespace OptionsPriceFinder.utils;

public class OptionSpreadCsv : CsvMapper<OptionSpread>
{
    private static readonly List<string> header = 
    [
        nameof(OptionSpread.symbol),
        nameof(OptionSpread.sharePrice),
        nameof(OptionSpread.expirationDate),
        nameof(OptionSpread.putCallRatio),
        nameof(OptionSpread.beta),
        nameof(OptionSpread.strikePriceSell),
        nameof(OptionSpread.optionPriceSell),
        nameof(OptionSpread.strikePriceBuy),
        nameof(OptionSpread.optionPriceBuy),
        nameof(OptionSpread.maximumLoss),
        nameof(OptionSpread.maximumGain),
        nameof(OptionSpread.maximumRatio),
        nameof(OptionSpread.deltaSell),
        nameof(OptionSpread.deltaBuy),
        nameof(OptionSpread.spreadValue)
    ];


    public OptionSpreadCsv(ILoggerFactory loggerFactory) : base(header, loggerFactory)
    {
    }
}