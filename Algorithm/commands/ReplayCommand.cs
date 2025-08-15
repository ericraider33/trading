using algorithm.algorithms;
using algorithm.model;
using algorithm.service;

namespace algorithm.commands;

public class ReplayCommand
{
    private readonly HistoryRepository repo;
    private readonly HistoryCalculations calculations;

    public ReplayCommand(HistoryRepository repo, HistoryCalculations calculations)
    {
        this.repo = repo;
        this.calculations = calculations;
    }
    
    public async Task run(string[] args)
    {
        Dictionary<string, List<History>> history = await repo.findAllHistory();
        List<string> symbols = history.Keys.Order().ToList();
        
        Console.WriteLine($"Replaying {symbols.Count} symbols history");
        IInvestment algorithm = new FixedInvestment();
        IInvestment buyAndHold = new BuyAndHoldInvestment();
        
        int num = 0;
        foreach (string symbol in symbols)
        {
            num++;

            List<History>? histories = history.GetValueOrDefault(symbol);
            if (histories == null || histories.Count <= 2)
                continue;
            
            DateTime startTime = histories.First().timestamp;
            DateTime endTime = histories.Last().timestamp;
            Decimal openPrice = histories.First().open;
            Decimal closePrice = histories.Last().close;

            DateTime startRange = endTime.AddYears(-1);
            if (startRange < startTime)
                continue;                                                   // not enough history
            
            calculations.calculateMovingAverages(histories);
            
            List<History> historyList = histories.Where(h => h.timestamp >= startRange).ToList();
            PositionInvestment initialInvestment = PositionInvestment.fromCash(symbol, 1_000_000m);
            PositionInvestment currentInvestment = algorithm.run(initialInvestment, historyList);
            PositionInvestment baselineInvestment = buyAndHold.run(initialInvestment, historyList);
            
            decimal gainsAlgorithm = currentInvestment.gains(initialInvestment, openPrice, closePrice);
            decimal gainsBaseline = baselineInvestment.gains(initialInvestment, openPrice, closePrice);
            
            string percentChange = gainsBaseline > 0 ? $"{gainsAlgorithm / gainsBaseline:P2}" : "N/A";
            Console.WriteLine($"{symbol} Gains: ${gainsAlgorithm} vs. ${gainsBaseline} or {percentChange} relative to buy-and-hold with {currentInvestment.transactions.Count} transactions");
        }
    }
}