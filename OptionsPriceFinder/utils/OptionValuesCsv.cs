using Microsoft.Extensions.Logging;
using OptionsPriceFinder.model;
using trading.util;

namespace OptionsPriceFinder.utils;

public class OptionValuesCsv : CsvMapper<OptionValues>
{
    private static readonly List<string> header = 
    [
        nameof(OptionValues.symbol),
        nameof(OptionValues.sharePrice),
        nameof(OptionValues.expirationDate),
        nameof(OptionValues.putCallRatio),
        nameof(OptionValues.beta),
        nameof(OptionValues.strikePrice1),
        nameof(OptionValues.strikePrice2),
        nameof(OptionValues.strikePrice3),
        nameof(OptionValues.optionPrice1),
        nameof(OptionValues.optionPrice2),
        nameof(OptionValues.optionPrice3),
        nameof(OptionValues.incomePercent1),
        nameof(OptionValues.incomePercent2),
        nameof(OptionValues.incomePercent3)
    ];

    public OptionValuesCsv(ILoggerFactory loggerFactory) : base(header, loggerFactory)
    {
    }
}