using Microsoft.Extensions.Logging;
using trading.util;

namespace algorithm.model;

public class HistoryCsv : CsvMapper<History>
{
    private static readonly List<string> header = 
    [
        nameof(History.timestamp),
        nameof(History.open),
        nameof(History.high),
        nameof(History.low),
        nameof(History.close),
        nameof(History.volume),
    ];

    public HistoryCsv(ILoggerFactory loggerFactory) : base(header, loggerFactory)
    {
    }
    
    public static bool isHistoryCsv(string toCheck)
    {
        return toCheck.StartsWith(nameof(History.timestamp));
    }
}