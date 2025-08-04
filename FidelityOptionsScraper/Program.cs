using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using FidelityOptionsScraper.Scrapers;
using FidelityOptionsScraper.Utils;
using pnyx.net.fluent;
using trading.util;

namespace FidelityOptionsScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Fidelity Options Scraper - Starting...");
            
        DirectoryUtil.changeToTradingDirectory();
        List<string> symbols;
        try
        {
            string inputFile = args.Length > 0 ? args[0] : "stocks.txt";
            
            using Pnyx p = new Pnyx();
            p.read(inputFile);
            p.hasLine();
            p.lineFilter(line => !line.StartsWith("#"));
            symbols = p.processCaptureLines();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error reading list of stocks: {ex.Message}");
            symbols = ["AAPL", "NFLX"];
        }

        // Output file path
        string outputPath = "options_prices.csv";
        DirectoryUtil.changeToTradingDirectory();
            
        // Initialize browser service
        BrowserService browserService = new BrowserService();
            
        try
        {
            Console.WriteLine("Initializing browser...");
                
            // Ask user if they want to use an existing session
            Console.WriteLine("\nDo you want to use an existing browser session? (y/n)");
            Console.WriteLine("Using an existing session is recommended for security and to avoid login issues.");
            bool useExistingSession = Console.ReadLine()?.Trim().ToLower() == "y";
                
            // Initialize browser
            bool initialized = await browserService.InitializeBrowserAsync(useExistingSession);
                
            if (!initialized)
            {
                Console.WriteLine("Failed to initialize browser. Exiting...");
                return;
            }
                
            // If not using existing session, handle login
            if (!useExistingSession)
            {
                Console.WriteLine("\nPlease enter your Fidelity credentials:");
                Console.Write("Username: ");
                string username = Console.ReadLine() ?? "";
                    
                Console.Write("Password: ");
                string password = ReadPassword();
                    
                bool loggedIn = await browserService.LoginIfNeededAsync(username, password);
                    
                if (!loggedIn)
                {
                    Console.WriteLine("Failed to log in to Fidelity. Exiting...");
                    return;
                }
            }
                
            // Initialize services
            StockScraperService stockScraper = new StockScraperService(browserService);
            OptionsScraperService optionsScraper = new OptionsScraperService(browserService);
            OptionsCalculationService calculationService = new OptionsCalculationService(stockScraper, optionsScraper);
                
            Console.WriteLine($"\nProcessing {symbols.Count} symbols: {string.Join(", ", symbols)}");
                
            // Process each symbol
            List<OptionData> results = new ();
            int num = 0;
            foreach (string symbol in symbols)
            {
                Console.WriteLine($"{++num} of {symbols.Count}]: Processing symbol: {symbol}");
                
                List<OptionData>? result = await calculationService.fetchOptionsForSymbol(symbol);
                if (result == null)
                    continue;
                    
                results.AddRange(result);
            }
                
            // Generate CSV output
            CsvGenerator.writeCsv(results, outputPath);
                
            Console.WriteLine("\nProcessing complete!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            // Ensure browser resources are properly disposed
            await browserService.DisposeAsync();
        }
            
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
        
    /// <summary>
    /// Reads a password from the console without displaying it
    /// </summary>
    private static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                break;
                
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password.Append(keyInfo.KeyChar);
                Console.Write("*");
            }
        }
            
        Console.WriteLine();
        return password.ToString();
    }
}