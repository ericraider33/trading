using AutoMapper;
using FidelityOptionsScraper.Utils;
using OptionsPriceFinder.model;
using pnyx.net.api;
using pnyx.net.fluent;

namespace OptionsPriceFinder.utils;

public class OptionValuesCsvGenerator
{
    private static readonly List<string> header = 
    [
        nameof(OptionValues.symbol),
        nameof(OptionValues.sharePrice),
        nameof(OptionValues.expirationDate),
        nameof(OptionValues.strikePrice1),
        nameof(OptionValues.strikePrice2),
        nameof(OptionValues.strikePrice3),
        nameof(OptionValues.callPrice1),
        nameof(OptionValues.callPrice2),
        nameof(OptionValues.callPrice3),
        nameof(OptionValues.incomePercent1),
        nameof(OptionValues.incomePercent2),
        nameof(OptionValues.incomePercent3)
    ];

    private static readonly IObjectConverterFromNameValuePair converter;
    
    static OptionValuesCsvGenerator()
    {
        MapperConfiguration config = new MapperConfiguration(cfg => {});
        converter = new AutoMapperObjectConverter<OptionValues>(config.CreateMapper());
    }
    
    /// <summary>
    /// Generates a CSV file from a list of option values
    /// </summary>
    public static void writeCsv(List<OptionValues> values, string outputPath)
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
    
    public static List<OptionValues> readCsv(string inputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.read(inputPath);
            p.parseCsv(hasHeader: true);
            p.rowToNameValuePair();
            p.nameValuePairToObject(converter);
            return p.processCaptureObject<OptionValues>();
        }
    }
}