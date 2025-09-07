using pnyx.net.api;
using pnyx.net.fluent;

namespace trading.util;

public abstract class CsvObjectMapper<T> where T : class
{
    public abstract List<string> getHeader();
    public abstract IObjectConverterFromNameValuePair getConverter();

    public void writeCsv(List<T> source, string outputPath)
    {
        using (Pnyx p = new Pnyx())
        {
            p.readObject(() => source);
            p.objectToNameValuePair(getConverter());
            p.nameValuePairToRow(header: getHeader());
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
        p.nameValuePairToObject(getConverter());
        return p.processCaptureObject<T>();
    }    
}