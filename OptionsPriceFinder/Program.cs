using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FidelityOptionsScraper.Models;
using FidelityOptionsScraper.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            Settings? settings = configuration.GetSection("settings").Get<Settings>();
            if (settings == null)
                throw new Exception("Settings not found");
            Settings.instance = settings;
            
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole();
                });
            using IHost host = hostBuilder.Build();
            ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();            

            DirectoryUtil.changeToTradingDirectory();

            string outputPath = "options_prices.csv";
            OptionDataCsv odCsv = new OptionDataCsv(loggerFactory);
            List<OptionData> options = odCsv.readCsv(outputPath);

            LocalTimestamp localTimestamp = LocalTimestamp.fromLocal(TimeZoneInfo.Local, DateTime.Now);
            LocalDay today = localTimestamp.day;
            LocalDay friday = DateCalculator.getNextFriday(today);
            
//            friday = friday.addDays(-1);
//            friday = LocalDay.fromLocal(TimeZoneInfo.Local, new DateTime(2025, 7, 18));
            
            OptionsCalculator calculator = new OptionsCalculator(1_000_000m);
            
            HashSet<string> symbols = calculator.symbols(options);
            List<OptionValues> callValuesList = new List<OptionValues>();
            List<OptionSpread> callSpreadList = new List<OptionSpread>();
            List<OptionValues> putValuesList = new List<OptionValues>();
            List<OptionSpread> putSpreadList = new List<OptionSpread>();
            foreach (string symbol in symbols)
            {
                OptionValues? callValues = calculator.calculateCallOptions(symbol, friday.local, options);
                if (callValues != null)
                    callValuesList.Add(callValues);

                List<OptionSpread>? callSpreads = calculator.calculateCallSpread(symbol, friday.local, options, limit: 10);
                if (callSpreads != null)
                    callSpreadList.AddRange(callSpreads);
                
                OptionValues? putValues = calculator.calculatePutOptions(symbol, friday.local, options);
                if (putValues != null)
                    putValuesList.Add(putValues);

                List<OptionSpread>? putSpreads = calculator.calculatePutSpread(symbol, friday.local, options, limit: 10);
                if (putSpreads != null)
                    putSpreadList.AddRange(putSpreads);
            }
            
            callValuesList = callValuesList.OrderByDescending(o => o.incomePercent1 ?? 0m).ToList();
            OptionValuesCsv ovGenerater = new OptionValuesCsv(loggerFactory);
            ovGenerater.writeCsv(callValuesList, $"call_values_{today.ToString()}.csv");
            
            callSpreadList = callSpreadList
                .Where(o => o.isStrikeFurtherThan(.03m))
                .OrderByDescending(o => o.spreadValue)
                .ToList();
            OptionSpreadCsv osGenerator = new OptionSpreadCsv(loggerFactory);
            osGenerator.writeCsv(callSpreadList, $"call_spreads_{today.ToString()}.csv");
            
            putValuesList = putValuesList.OrderByDescending(o => o.incomePercent1 ?? 0m).ToList();
            ovGenerater.writeCsv(putValuesList, $"put_values_{today.ToString()}.csv");

            putSpreadList = putSpreadList.OrderByDescending(o => o.spreadValue).ToList();
            osGenerator.writeCsv(putSpreadList, $"put_spreads_{today.ToString()}.csv");
            
            Console.WriteLine("DONE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
