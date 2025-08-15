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
    public decimal targetWeightHigh { get; set; } = 0.50m;
    public decimal targetWeightLow { get; set; } = 1m;
    
    public int minimumTransactionQuantity { get; set; } = 1;             // minimum transaction size in shares
    public decimal maximumTransactionPercentage { get; set; } = 0.01m;   // minimum transaction size in dollars
    
    public virtual PositionInvestment run(PositionInvestment initialInvestment, List<History> historyList)
    {
        PositionInvestment result = (PositionInvestment)initialInvestment.Clone();

        bool initialized = false;
        foreach (History history in historyList)
        {
            if (!initialized)
            {
                initialize(result, history);
                initialized = true;
                continue;
            }
            
            balance(result, history);
        }

        return result;
    }

    private void initialize(PositionInvestment investment, History history)
    {
        decimal cashValue = investment.cash.quantity;
        decimal stockValue = investment.position.quantity * history.open;
        decimal totalValue = cashValue + stockValue;
    
        decimal idealValue = totalValue * (targetWeightHigh + targetWeightLow) / 2;
        int idealQuantity = (int)Math.Floor(idealValue / history.open);

        int toBuy = idealQuantity - (int)investment.position.quantity;
        decimal toBuyValue = toBuy * history.open;
    
        if (toBuyValue > investment.cash.quantity)
            toBuy = (int)Math.Floor(investment.cash.quantity / history.open);   // buy as much as we can
    
        investment.buy(toBuy, history.open, history.timestamp);
    }
    

    private void balance(PositionInvestment investment, History history)
    {
        decimal cashValue = investment.cash.quantity;
        decimal stockValue = investment.position.quantity * history.open;
        decimal totalValue = cashValue + stockValue;
        
        decimal weight = stockValue / totalValue;
        
        // need to sell some stock
        if (weight > targetWeightHigh)
        {
            int idealQuantity = (int)Math.Floor(totalValue * targetWeightHigh / history.open);

            if (history.isAboveMovingAverage21And49())
            {
                int toSell = (int)investment.position.quantity - idealQuantity;
                if (toSell < minimumTransactionQuantity)
                    return;                                                         // not enough to sell

                decimal toSellValue = toSell * history.open;
                decimal limit = totalValue * maximumTransactionPercentage;
                if (toSellValue > limit)
                    toSell = Math.Max(1, (int)Math.Floor(limit / history.open));    // sell as much as we can
                
                investment.sell(toSell, history.open, history.timestamp);
                return;
            }    
        }
        
        // need to buy some stock
        if (weight < targetWeightLow)
        {
            int idealQuantity = (int)Math.Floor(totalValue * targetWeightLow / history.open);
            
            if (history.isBelowMovingAverage21And49())
            {
                int toBuy = idealQuantity - (int)investment.position.quantity;
                if (toBuy < minimumTransactionQuantity)
                    return;                                                 // not enough to buy
            
                decimal toBuyValue = toBuy * history.open;
                if (toBuyValue > investment.cash.quantity)
                    toBuy = (int)Math.Floor(investment.cash.quantity / history.open);   // buy as much as we can

                if (toBuy < minimumTransactionQuantity)
                    return;                                                 // not enough to buy

                toBuyValue = toBuy * history.open;
                decimal limit = totalValue * maximumTransactionPercentage;
                if (toBuyValue > limit)
                    toBuy = Math.Max(1, (int)Math.Floor(limit / history.open));        // buy as much as we can
                
                investment.buy(toBuy, history.open, history.timestamp);
                return;
            }
        }
    }
}