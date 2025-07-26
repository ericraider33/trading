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
            LocalDay today = localTimestamp.day;
            LocalDay friday = DateCalculator.getNextFriday(today);
            
//            friday = friday.addDays(-1);
            friday = LocalDay.fromLocal(TimeZoneInfo.Local, new DateTime(2025, 7, 18));
            
            OptionsCalculator calculator = new OptionsCalculator(100000m);
            
            HashSet<string> symbols = calculator.symbols(options);
            List<OptionValues> callValuesList = new List<OptionValues>();
            List<OptionValues> putValuesList = new List<OptionValues>();
            List<OptionSpread> putSpreadList = new List<OptionSpread>();
            foreach (string symbol in symbols)
            {
                OptionValues? callValues = calculator.calculateCallOptions(symbol, friday.local, options);
                if (callValues != null)
                    callValuesList.Add(callValues);
                
                OptionValues? putValues = calculator.calculatePutOptions(symbol, friday.local, options);
                if (putValues != null)
                    putValuesList.Add(putValues);

                OptionSpread? putSpread = calculator.calculatePutSpread(symbol, friday.local, options);
                if (putSpread != null)
                    putSpreadList.Add(putSpread);
            }
            
            callValuesList = callValuesList.OrderByDescending(o => o.incomePercent1 ?? 0m).ToList();
            OptionValuesCsvGenerator.writeCsv(callValuesList, $"call_values_{today.ToString()}.csv");
            
            putValuesList = putValuesList.OrderByDescending(o => o.incomePercent1 ?? 0m).ToList();
            OptionValuesCsvGenerator.writeCsv(putValuesList, $"put_values_{today.ToString()}.csv");

            putSpreadList = putSpreadList.OrderBy(o => o.maximumRatio).ToList();
            OptionSpreadCsvGenerator.writeCsv(putSpreadList, $"put_spreads_{today.ToString()}.csv");
            
            Console.WriteLine("DONE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
