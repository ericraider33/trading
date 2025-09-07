using OptionsPriceFinder.model;
using pnyx.net.api;
using trading.util;

namespace OptionsPriceFinder.utils;

public class OptionDateGainCsv : CsvObjectMapper<OptionDateGain>
{
    private readonly OptionDateGainConverter converter;
    
    public OptionDateGainCsv(int dateCount = 5)
    {
        converter = new OptionDateGainConverter(dateCount);
    }
    
    public override List<string> getHeader()
    {
        return converter.getHeaders();
    }

    public override IObjectConverterFromNameValuePair getConverter()
    {
        return converter;
    }
}