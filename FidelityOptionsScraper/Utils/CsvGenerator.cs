using System.Text;
using AutoMapper;
using FidelityOptionsScraper.Models;
using pnyx.net.api;
using pnyx.net.fluent;

namespace FidelityOptionsScraper.Utils;

public static class CsvGenerator
{
    /// <summary>
    /// Generates a CSV file from a list of options
    /// </summary>
    /// <param name="results">List of options</param>
    /// <param name="outputPath">Path to save the CSV file</param>
    public static void GenerateCsv(List<OptionData> options, string outputPath)
    {
        MapperConfiguration config = new MapperConfiguration(cfg =>
        {
        });

        IObjectConverterFromNameValuePair converter = new AutoMapperObjectConverter<OptionData>
        {
            mapper = config.CreateMapper() 
        };
        
        List<IDictionary<String, Object>> actual;
        using (Pnyx p = new Pnyx())
        {
            p.objectToNameValuePair(converter);
            p.writeCsv(outputPath);
        }
        
            
        Console.WriteLine($"CSV file saved to: {Path.GetFullPath(outputPath)}");
    }
}