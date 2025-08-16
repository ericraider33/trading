using System.Diagnostics;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using FidelityOptionsScraper.Scrapers;
using FidelityOptionsScraper.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using trading.util;

namespace FidelityOptionsScraper;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Fidelity Options Scraper - Starting...");

        // Load configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();

        Settings? settings = configuration.GetSection("settings").Get<Settings>();
        if (settings == null)
            throw new Exception("Settings not found");
        Settings.instance = settings;
        
        // Output file path
        string outputPath = "options_prices.csv";
        DirectoryUtil.changeToTradingDirectory();
        
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddAutoMapper(cfg =>
                {
                    // Set your license key here
                    cfg.LicenseKey = Settings.instance.autoMapperApiKey;
                });

                services.AddSingleton<StockBetaCsv>();
                services.AddSingleton<StockBetaRepository>();
                services.AddSingleton<BrowserService>();
                services.AddSingleton<StockScraperService>();
                services.AddSingleton<OptionsScraperService>();
                services.AddSingleton<OptionsCalculationService>();
                services.AddSingleton<OptionDataCsv>();
            });            
        using IHost host = hostBuilder.Build();

        // Reads list of ticker symbols
        string inputFile = args.Length > 0 ? args[0] : "stocks.txt";
        List<string> symbols = await StockUtil.readSymbols(inputFile);
            
        // Initialize browser service
        BrowserService browserService = host.Services.GetRequiredService<BrowserService>();
            
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
                
            OptionsCalculationService calculationService = host.Services.GetRequiredService<OptionsCalculationService>();
            Console.WriteLine($"\nProcessing {symbols.Count} symbols: {string.Join(", ", symbols)}");
                
            // Process each symbol
            List<OptionData> results = new ();
            int num = 0;
            foreach (string symbol in symbols)
            {
                Console.WriteLine($"{++num} of {symbols.Count}): Processing symbol: {symbol}");
                
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<OptionData>? result = await calculationService.fetchOptionsForSymbol(symbol);
                if (result == null)
                    continue;
                    
                results.AddRange(result);
                stopwatch.Stop();
                Console.WriteLine($"Processed {symbol} in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            }
                
            // Generate CSV output
            OptionDataCsv odGenerator = host.Services.GetRequiredService<OptionDataCsv>();
            odGenerator.writeCsv(results, outputPath);
                
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