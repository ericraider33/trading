using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Utils;
using Microsoft.Extensions.Configuration;
using OptionsPriceFinder.model;
using OptionsPriceFinder.services;
using OptionsPriceFinder.utils;
using pnyx.net.util.dates;
using trading.util;

namespace OptionsPriceFinder;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Options Price Finder - Starting...");

            // Load configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            DirectoryUtil.changeToTradingDirectory();

            string outputPath = "options_prices.csv";
            List<OptionData> options = CsvGenerator.readCsv(outputPath);

            LocalTimestamp localTimestamp = LocalTimestamp.fromLocal(TimeZoneInfo.Local, DateTime.Now);
            LocalDay friday = DateCalculator.getNextFriday(localTimestamp.day);
            
//            friday = friday.addDays(-1);
            
            OptionsCalculator calculator = new OptionsCalculator(100000m);
            
            HashSet<string> symbols = calculator.symbols(options);
            List<OptionValues> optionValuesList = new List<OptionValues>();
            foreach (string symbol in symbols)
            {
                OptionValues? values = calculator.calculateOption(symbol, friday.local, options);
                if (values != null)
                    optionValuesList.Add(values);
            }
            
            optionValuesList = optionValuesList.OrderByDescending(o => o.incomePercent1 ?? 0m).ToList();
            OptionValuesCsvGenerator.writeCsv(optionValuesList, "options_values.csv");
            Console.WriteLine("DONE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
