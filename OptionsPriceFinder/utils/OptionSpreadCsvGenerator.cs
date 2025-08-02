using AutoMapper;
using FidelityOptionsScraper.Utils;
using OptionsPriceFinder.model;
using pnyx.net.api;
using pnyx.net.fluent;

namespace OptionsPriceFinder.utils;

public class OptionSpreadCsvGenerator
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

    private static readonly IObjectConverterFromNameValuePair converter;
    
    static OptionSpreadCsvGenerator()
    {
        MapperConfiguration config = new MapperConfiguration(cfg => {});
        converter = new AutoMapperObjectConverter<OptionSpread>(config.CreateMapper());
    }
    
    public static void writeCsv(List<OptionSpread> values, string outputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.readObject(() => values);
            p.objectToNameValuePair(converter);
            p.nameValuePairToRow(header: header);
            p.writeCsv(outputPath);
        }
            
        Console.WriteLine($"CSV file saved to: {Path.GetFullPath(outputPath)}");
    }
    
    public static List<OptionSpread> readCsv(string inputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.read(inputPath);
            p.parseCsv(hasHeader: true);
            p.rowToNameValuePair();
            p.nameValuePairToObject(converter);
            return p.processCaptureObject<OptionSpread>();
        }
    }
}