using algorithm.model;
using Flurl;
using Flurl.Http;
using trading.util;

namespace algorithm.commands;

public class HistoryCommand
{
    public static async Task run(string[] args)
    {
        // Reads list of ticker symbols
        string inputFile = args.Length > 0 ? args[0] : "stocks.txt";
        List<string> symbols = await StockUtil.readSymbols(inputFile);

        Console.WriteLine($"\nProcessing {symbols.Count} symbols: {string.Join(", ", symbols)}");

        int num = 0;
        foreach (string symbol in symbols)
        {
            num++;
            
            string outputFile = $"history_{symbol.ToLower()}.csv";
            if (File.Exists(outputFile))
            {
                Console.WriteLine($"{num} of {symbols.Count}): Skipping {symbol} because it already exists");
                continue;
            }
            
            Console.WriteLine($"{num} of {symbols.Count}): Getting weekly history for stock={symbol}");
            string csv = await getWeeklyTimeSeriesCsv(symbol);

            await File.WriteAllTextAsync(outputFile, csv);
        }
    }

    private static async Task<string> getWeeklyTimeSeriesCsv(string symbol)
    {
        // Use Flurl to build the URL cleanly and make the GET request
        try
        {
            string csvData = await "https://www.alphavantage.co"
                .AppendPathSegment("query")
                .SetQueryParam("function", "TIME_SERIES_WEEKLY")
                .SetQueryParam("symbol", symbol)
                .SetQueryParam("apikey", Settings.instance.alphaVantageApiKey)
                .SetQueryParam("datatype", "csv")
                .GetStringAsync();

            if (!HistoryCsv.isHistoryCsv(csvData))
                throw new Exception($"Invalid csv data: {csvData}");
            
            return csvData;
        }
        catch (FlurlHttpException ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            throw new HttpRequestException($"HTTP request failed: {ex.Message}", ex);
        }
    }    
}
