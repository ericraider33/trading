using pnyx.net.fluent;

namespace trading.util;

public static class StockUtil
{
    public static async Task<List<string>> readSymbols(string inputFile, string[]? defaults = null)
    {
        try
        {
            using Pnyx p = new Pnyx();
            p.read(inputFile);
            p.hasLine();
            p.lineFilter(line => !line.StartsWith("#"));
            return p.processCaptureLines();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error reading list of stocks: {ex.Message}");
            if (defaults != null)
                return defaults.ToList();
                
            return ["AAPL", "NFLX"];
        }
    }
}