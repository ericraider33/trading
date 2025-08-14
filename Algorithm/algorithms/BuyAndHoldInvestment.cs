using algorithm.model;

namespace algorithm.algorithms;

public class BuyAndHoldInvestment : FixedInvestment, IInvestment
{
    public BuyAndHoldInvestment()
    {
        targetWeight = 1.0m;
        minimumTransactionQuantity = 1;
        minimumTransactionValue = 0m;
    }
    
    public override PositionInvestment run(PositionInvestment initialInvestment, List<History> historyList)
    {
        PositionInvestment result = (PositionInvestment)initialInvestment.Clone();
        
        if (historyList.Count > 0)
            balance(result, historyList.First());

        return result;
    }
}