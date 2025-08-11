using AutoMapper;
using Microsoft.Extensions.Logging;
using pnyx.net.api;
using pnyx.net.fluent;

namespace trading.util;

public class CsvMapper<T> where T : class
{
    private readonly List<string> header;
    private readonly MapperConfiguration config;
    private readonly IObjectConverterFromNameValuePair converter;

    public CsvMapper(List<string> header, ILoggerFactory loggerFactory)
    {
        this.header = header;
        config = new MapperConfiguration(cfg => {}, loggerFactory);        
        converter = new AutoMapperObjectConverter<T>(config.CreateMapper());
    }
    
    public void writeCsv(List<T> source, string outputPath)
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
    
    public List<T> readCsv(string inputPath)
    {
        using Pnyx p = new Pnyx();
        p.read(inputPath);
        p.parseCsv(hasHeader: true);
        p.rowToNameValuePair();
        p.nameValuePairToObject(converter);
        return p.processCaptureObject<T>();
    }    
}