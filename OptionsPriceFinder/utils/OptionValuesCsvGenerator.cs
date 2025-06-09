using AutoMapper;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Utils;
using OptionsPriceFinder.model;
using pnyx.net.api;
using pnyx.net.fluent;

namespace OptionsPriceFinder.utils;

public static class OptionValuesCsvGenerator
{
    private static readonly List<string> header = 
    [
        nameof(OptionValues.symbol),
        nameof(OptionValues.sharePrice),
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
    /// Generates a CSV file from a list of options
    /// </summary>
    /// <param name="options">List of option values</param>
    /// <param name="outputPath">Path to save the CSV file</param>
    public static void writeCsv(List<OptionValues> options, string outputPath)
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
        using Pnyx p = new Pnyx();
        p.read(inputPath);
        p.parseCsv(hasHeader: true);
        p.rowToNameValuePair();
        p.nameValuePairToObject(converter);
        return p.processCaptureObject<OptionData>();
    }
}