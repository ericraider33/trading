namespace trading.util;

public class Settings
{
    public static Settings instance { get; set; }
    
    public string alphaVantageApiKey { get; set; }
    public string autoMapperApiKey { get; set; }
}