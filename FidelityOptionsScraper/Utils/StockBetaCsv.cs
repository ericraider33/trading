using FidelityOptionsScraper.Models;
using Microsoft.Extensions.Logging;
using trading.util;

namespace FidelityOptionsScraper.Utils;

public class StockBetaCsv : CsvMapper<StockBeta>
{
    private static readonly List<string> header =
    [
        nameof(StockBeta.symbol),
        nameof(StockBeta.beta),
    ];
    
    public StockBetaCsv(ILoggerFactory loggerFactory) : base(header, loggerFactory)
    {
    }
}