// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Algorithm;
using Algorithm.commands;
using Microsoft.Extensions.Configuration;
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

try
{
    string[] subArgs = args.Skip(1).ToArray();
    Stopwatch timer = Stopwatch.StartNew();
    switch (args[0].ToLower())
    {
        case "history": await HistoryCommand.run(subArgs); break;
        default: 
            throw new Exception($"Unknown command: {args[0]}");
    }
    
    timer.Stop();
    Console.WriteLine($"\nProcessing complete in {timer.Elapsed.TotalSeconds:0.0} ms");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
    