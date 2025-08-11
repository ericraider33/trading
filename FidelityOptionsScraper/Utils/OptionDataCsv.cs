using FidelityOptionsScraper.Models;
using Microsoft.Extensions.Logging;
using trading.util;

namespace FidelityOptionsScraper.Utils;

public class OptionDataCsv : CsvMapper<OptionData>
{
    private static readonly List<string> header = 
    [
        nameof(OptionData.symbol),
        nameof(OptionData.sharePrice),
        nameof(OptionData.beta),
        nameof(OptionData.strikePrice),
        nameof(OptionData.expirationDate),
        nameof(OptionData.putCallRatio),
        nameof(OptionData.callLastPrice),
        nameof(OptionData.callBidPrice),
        nameof(OptionData.callAskPrice),
        nameof(OptionData.callOpenInterest),
        nameof(OptionData.callImpliedVolatility),
        nameof(OptionData.callDelta),
        nameof(OptionData.putLastPrice),
        nameof(OptionData.putBidPrice),
        nameof(OptionData.putAskPrice),
        nameof(OptionData.putOpenInterest),
        nameof(OptionData.putImpliedVolatility),
        nameof(OptionData.putDelta),
    ];

    public OptionDataCsv(ILoggerFactory loggerFactory) : base(header, loggerFactory)
    {
    }
}