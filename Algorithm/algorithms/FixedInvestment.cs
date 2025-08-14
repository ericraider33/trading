using algorithm.model;

namespace algorithm.algorithms;

/// <summary>
/// An algorithm that maintains a fixed investment in a stock. As it goes it, the algorithm will sell the stock.
/// As it falls, the algorithm will buy more of the stock.
/// </summary>
public class FixedInvestment : IInvestment
{
    /// <summary>
    /// Target weight of the stock in this investment strategy.
    /// </summary>
    public decimal targetWeight { get; set; } = 0.8m;
    
    public int minimumTransactionQuantity { get; set; } = 1;        // minimum transaction size in shares
    public decimal minimumTransactionValue { get; set; } = 100m;   // minimum transaction size in dollars
    
    public PositionInvestment run(PositionInvestment initialInvestment, List<History> historyList)
    {
        PositionInvestment result = (PositionInvestment)initialInvestment.Clone();

        foreach (History history in historyList)
        {
            balance(result, history);
        }

        return result;
    }

    private void balance(PositionInvestment investment, History history)
    {
        decimal cashValue = investment.cash.quantity;
        decimal stockValue = investment.position.quantity * history.open;
        decimal totalValue = cashValue + stockValue;
        
        decimal weight = stockValue / totalValue;
        
        decimal idealValue = totalValue * targetWeight;
        int idealQuantity = (int)Math.Floor(idealValue / history.open);

        // need to sell some stock
        if (weight > targetWeight)
        {
            int toSell = (int)investment.position.quantity - idealQuantity;
            decimal toSellValue = toSell * history.open;
            
            if (toSell < minimumTransactionQuantity || toSellValue < minimumTransactionValue)
                return;                                                 // not enough to sell
            
            investment.sell(toSell, history.open, history.timestamp);
        }
        // need to buy some stock
        else if (weight < targetWeight)
        {
            int toBuy = idealQuantity - (int)investment.position.quantity;
            decimal toBuyValue = toBuy * history.open;
            
            if (toBuy < minimumTransactionQuantity || toBuyValue < minimumTransactionValue)
                return;                                                 // not enough to buy
            
            if (toBuyValue > investment.cash.quantity)
                toBuy = (int)Math.Floor(investment.cash.quantity / history.open);   // buy as much as we can
            
            investment.buy(toBuy, history.open, history.timestamp);
        }
    }
}