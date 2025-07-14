using AutoMapper;
using FidelityOptionsScraper.Models;
using pnyx.net.api;
using pnyx.net.fluent;

namespace FidelityOptionsScraper.Utils;

public static class CsvGenerator
{
    private static readonly List<string> header = 
    [
        nameof(OptionData.symbol),
        nameof(OptionData.sharePrice),
        nameof(OptionData.strikePrice),
        nameof(OptionData.expirationDate),
        nameof(OptionData.putCallRatio),
        nameof(OptionData.callLastPrice),
        nameof(OptionData.callBidPrice),
        nameof(OptionData.callAskPrice),
        nameof(OptionData.putLastPrice),
        nameof(OptionData.putBidPrice),
        nameof(OptionData.putAskPrice),
        nameof(OptionData.beta),
    ];

    private static readonly IObjectConverterFromNameValuePair converter;
    
    static CsvGenerator()
    {
        MapperConfiguration config = new MapperConfiguration(cfg => {});
        converter = new AutoMapperObjectConverter<OptionData>(config.CreateMapper());
    }
    
    /// <summary>
    /// Generates a CSV file from a list of options
    /// </summary>
    /// <param name="results">List of options</param>
    /// <param name="outputPath">Path to save the CSV file</param>
    public static void writeCsv(List<OptionData> options, string outputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.readObject(() => options);
            p.objectToNameValuePair(converter);
            p.nameValuePairToRow(header: header);
            p.writeCsv(outputPath);
        }
            
        Console.WriteLine($"CSV file saved to: {Path.GetFullPath(outputPath)}");
    }
    
    public static List<OptionData> readCsv(string inputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.read(inputPath);
            p.parseCsv(hasHeader: true);
            p.rowToNameValuePair();
            p.nameValuePairToObject(converter);
            return p.processCaptureObject<OptionData>();
        }
    }
}