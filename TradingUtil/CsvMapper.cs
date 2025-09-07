using AutoMapper;
using Microsoft.Extensions.Logging;
using pnyx.net.api;

namespace trading.util;

public class CsvMapper<T> : CsvObjectMapper<T> where T : class
{
    private readonly List<string> header;
    private readonly IObjectConverterFromNameValuePair converter;

    public CsvMapper(List<string> header, ILoggerFactory loggerFactory)
    {
        this.header = header;

        MapperConfiguration config = new MapperConfiguration(cfg => {}, loggerFactory);        
        converter = new AutoMapperObjectConverter<T>(config.CreateMapper());
    }
    
    public override List<string> getHeader()
    {
        return header;
    }
    
    public override IObjectConverterFromNameValuePair getConverter()
    {
        return converter;
    }
}