using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Services;
using FidelityOptionsScraper.Scrapers;
using FidelityOptionsScraper.Utils;

namespace FidelityOptionsScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Fidelity Options Scraper - Starting...");
            
            // Define test symbols (can be overridden by command line args)
            string[] symbols = { "AAPL", "TSLA", "NFLX" };
            if (args.Length > 0)
            {
                symbols = args;
            }

            // Output file path
            string outputPath = "options_prices.csv";
            
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
                
                Console.WriteLine($"\nProcessing symbols: {string.Join(", ", symbols)}");
                
                // Process each symbol
                List<OptionResult> results = new ();
                foreach (string symbol in symbols)
                {
                    OptionResult? result = await calculationService.calculateOptionsForSymbol(symbol);
                    if (result == null)
                        continue;
                    
                    results.Add(result);
                }
                
                // Generate CSV output
                CsvGenerator.GenerateCsv(results, outputPath);
                
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
}
