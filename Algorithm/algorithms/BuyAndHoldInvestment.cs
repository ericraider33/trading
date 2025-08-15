using algorithm.model;

namespace algorithm.algorithms;

public class BuyAndHoldInvestment : IInvestment
{
    public PositionInvestment run(PositionInvestment initialInvestment, List<History> historyList)
    {
        PositionInvestment result = (PositionInvestment)initialInvestment.Clone();
        
        if (historyList.Count > 0)
            balance(result, historyList.First());

        return result;
    }
    
    private void balance(PositionInvestment investment, History history)
    {
        decimal cashValue = investment.cash.quantity;
        decimal stockValue = investment.position.quantity * history.open;
        decimal totalValue = cashValue + stockValue;
        
        decimal weight = stockValue / totalValue;
        
        decimal idealValue = totalValue;
        int idealQuantity = (int)Math.Floor(idealValue / history.open);

        // need to sell some stock
        if (weight > 1m)
        {
            int toSell = (int)investment.position.quantity - idealQuantity;
            investment.sell(toSell, history.open, history.timestamp);
        }
        // need to buy some stock
        else if (weight < 1m)
        {
            int toBuy = idealQuantity - (int)investment.position.quantity;
            decimal toBuyValue = toBuy * history.open;
            
            if (toBuyValue > investment.cash.quantity)
                toBuy = (int)Math.Floor(investment.cash.quantity / history.open);   // buy as much as we can
            
            investment.buy(toBuy, history.open, history.timestamp);
        }
    }
}