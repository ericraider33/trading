using Microsoft.Extensions.Configuration;
using OptionsPriceFinder.Models;
using OptionsPriceFinder.Services;
using OptionsPriceFinder.Utils;

namespace OptionsPriceFinder;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Options Price Finder - Starting...");

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            string apiKey = configuration["PolygonApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                Console.WriteLine("Please set your Polygon.io API key in appsettings.json");
                return;
            }

            // Setup HTTP client
            var httpClient = new HttpClient();
            var polygonClient = new PolygonApiClient(httpClient, apiKey);

            // Setup services
            var stockService = new StockService(polygonClient);
            var optionsService = new OptionsService(polygonClient);
            var calculationService = new OptionsCalculationService(stockService, optionsService);

            // Define test symbols
            string[] symbols = { "AAPL", "TSLA", "NFLX" };
            if (args.Length > 0)
            {
                symbols = args;
            }

            Console.WriteLine($"Processing symbols: {string.Join(", ", symbols)}");

            // Process each symbol
            var results = new List<OptionResult>();
            foreach (var symbol in symbols)
            {
                Console.WriteLine($"Processing {symbol}...");
                var result = await calculationService.CalculateOptionsForSymbolAsync(symbol);
                results.Add(result);
                    
                // Display results for verification
                Console.WriteLine($"  Current Price: {result.CurrentPrice}");
                Console.WriteLine($"  Friday Date: {result.FridayDate}");
                Console.WriteLine($"  Call Option 1%: {result.CallOption1Percent}");
                Console.WriteLine($"  Call Option 2%: {result.CallOption2Percent}");
                Console.WriteLine($"  Call Option 3%: {result.CallOption3Percent}");
            }

            // Generate CSV output
            string outputPath = "options_prices.csv";
            CsvGenerator.GenerateCsv(results, outputPath);

            Console.WriteLine($"CSV output saved to: {Path.GetFullPath(outputPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
