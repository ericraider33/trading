using algorithm.model;
using algorithm.service;

namespace algorithm.commands;

public class ReplayCommand
{
    private readonly HistoryRepository repo;

    public ReplayCommand(HistoryRepository repo)
    {
        this.repo = repo;
    }
    
    public async Task run(string[] args)
    {
        Dictionary<string, List<History>> history = await repo.findAllHistory();
        List<string> symbols = history.Keys.Order().ToList();
        
        Console.WriteLine($"Replaying {symbols.Count} symbols history");

        int num = 0;
        foreach (string symbol in symbols)
        {
            num++;

            Console.WriteLine($"{num} of {symbols.Count}) Replaying history for stock={symbol}");
        }
    }
}