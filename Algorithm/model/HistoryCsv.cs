using AutoMapper;
using pnyx.net.api;
using pnyx.net.fluent;
using trading.util;

namespace Algorithm.model;

public class HistoryCsv
{
    public static bool isHistoryCsv(string toCheck)
    {
        return toCheck.StartsWith(nameof(History.timestamp));
    }
    
    private static readonly List<string> header = 
    [
        nameof(History.timestamp),
        nameof(History.open),
        nameof(History.high),
        nameof(History.low),
        nameof(History.close),
        nameof(History.volume),
    ];
    
    private static readonly IObjectConverterFromNameValuePair converter;
    
    static HistoryCsv()
    {
        MapperConfiguration config = new MapperConfiguration(cfg => {});
        converter = new AutoMapperObjectConverter<History>(config.CreateMapper());
    }
    
    public static void writeCsv(List<History> source, string outputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.readObject(() => source);
            p.objectToNameValuePair(converter);
            p.nameValuePairToRow(header: header);
            p.writeCsv(outputPath);
        }
            
        Console.WriteLine($"CSV file saved to: {Path.GetFullPath(outputPath)}");
    }
    
    public static List<History> readCsv(string inputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.read(inputPath);
            p.parseCsv(hasHeader: true);
            p.rowToNameValuePair();
            p.nameValuePairToObject(converter);
            return p.processCaptureObject<History>();
        }
    }    
}