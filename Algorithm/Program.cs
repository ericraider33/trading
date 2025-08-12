// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using algorithm.commands;
using algorithm.model;
using algorithm.service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using trading.util;

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

DirectoryUtil.changeToTradingDirectory();

if (args.Length == 0)
{
    Console.WriteLine("Please specify the command to run");
    Environment.Exit(1);
}

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
        
        services.AddSingleton<HistoryCsv>();
        services.AddSingleton<HistoryRepository>();
        services.AddSingleton<ReplayCommand>();
    });            
using IHost host = hostBuilder.Build();

try
{
    string[] subArgs = args.Skip(1).ToArray();
    Stopwatch timer = Stopwatch.StartNew();
    switch (args[0].ToLower())
    {
        case "history": await HistoryCommand.run(subArgs); break;
        case "replay": await host.Services.GetRequiredService<ReplayCommand>().run(subArgs); break;
        default: 
            throw new Exception($"Unknown command: {args[0]}");
    }
    
    timer.Stop();
    Console.WriteLine($"\nProcessing complete in {timer.Elapsed.TotalSeconds:0.0} ms");
}
catch (Exception ex)
{
    while (ex.InnerException != null)
        ex = ex.InnerException;
    
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
    