using FidelityOptionsScraper.Models;
using OptionsPriceFinder.model;

namespace OptionsPriceFinder.services;

public class OptionsCalculator
{
    private readonly decimal investmentAmount;
    
    public OptionsCalculator(decimal investmentAmount)
    {
        this.investmentAmount = investmentAmount;
    }
    
    public HashSet<string> symbols(List<OptionData> options)
    {
        return options.Select(x => x.symbol)
            .Distinct()
            .ToHashSet();
    }
    
    public OptionValues? calculateCallOptions(string symbol, DateTime expirationDate, List<OptionData> options)
    {
        List<OptionData> toReview = options
            .Where(x => x.symbol == symbol && x.expirationDate == expirationDate)
            .OrderBy(x => x.strikePrice)
            .ToList();

        if (toReview.Count == 0)
            return null;

        OptionValues values = calculateBasis(toReview[0]);
        if (values.shares == 0)
            return null;

        bool fractional = hasFactionalStrikePrice(options);
        
        decimal precent1 = roundPrice(1.01m * values.sharePrice, fractional);
        decimal precent2 = roundPrice(1.02m * values.sharePrice, fractional);
        decimal precent3 = roundPrice(1.03m * values.sharePrice, fractional);
       
        int? index1 = findGreaterThanOrEqualIndex(toReview, precent1);
        int? index3 = findLesserThanOrEqualIndex(toReview, precent3);
        int? index2 = null;
        
        if (index3 != null && index1 != null)
        {
            if (index3 > index1+1)
                index2 = (index1 + index3) / 2;
        }
        else
        {
            index2 = findGreaterThanOrEqualIndex(toReview, precent2);
        }
        
        OptionData? option1 = index1 != null ? toReview[index1.Value] : null;
        OptionData? option2 = index2 != null ? toReview[index2.Value] : null;
        OptionData? option3 = index3 != null ? toReview[index3.Value] : null;
        
        values.strikePrice1 = option1?.strikePrice; 
        values.strikePrice2 = option2?.strikePrice;
        values.strikePrice3 = option3?.strikePrice;
        
        values.optionPrice1 = option1?.callLastPrice;
        values.optionPrice2 = option2?.callLastPrice;
        values.optionPrice3 = option3?.callLastPrice;

        values.incomePercent1 = percentIncome(values.optionPrice1, values.sharePrice);
        values.incomePercent2 = percentIncome(values.optionPrice2, values.sharePrice);
        values.incomePercent3 = percentIncome(values.optionPrice3, values.sharePrice);

        return values;
    }
    
    public OptionValues? calculatePutOptions(string symbol, DateTime expirationDate, List<OptionData> options)
    {
        List<OptionData> toReview = options
            .Where(x => x.symbol == symbol && x.expirationDate == expirationDate)
            .OrderBy(x => x.strikePrice)
            .ToList();

        if (toReview.Count == 0)
            return null;

        OptionValues values = calculateBasis(toReview[0]);
        if (values.shares == 0)
            return null;

        bool fractional = hasFactionalStrikePrice(options);
        
        decimal precent1 = roundPrice(0.99m * values.sharePrice, fractional);
        decimal precent2 = roundPrice(0.98m * values.sharePrice, fractional);
        decimal precent3 = roundPrice(0.97m * values.sharePrice, fractional);
       
        int? index1 = findLesserThanOrEqualIndex(toReview, precent1);
        int? index3 = findGreaterThanOrEqualIndex(toReview, precent3);
        int? index2 = null;
        
        if (index3 != null && index1 != null)
        {
            if (index1 > index3+1)
                index2 = (index1 + index3) / 2;
        }
        else
        {
            index2 = findLesserThanOrEqualIndex(toReview, precent2);
        }
        
        OptionData? option1 = index1 != null ? toReview[index1.Value] : null;
        OptionData? option2 = index2 != null ? toReview[index2.Value] : null;
        OptionData? option3 = index3 != null ? toReview[index3.Value] : null;
        
        values.strikePrice1 = option1?.strikePrice; 
        values.strikePrice2 = option2?.strikePrice;
        values.strikePrice3 = option3?.strikePrice;
        
        values.optionPrice1 = option1?.putLastPrice;
        values.optionPrice2 = option2?.putLastPrice;
        values.optionPrice3 = option3?.putLastPrice;

        values.incomePercent1 = percentIncome(values.optionPrice1, values.sharePrice);
        values.incomePercent2 = percentIncome(values.optionPrice2, values.sharePrice);
        values.incomePercent3 = percentIncome(values.optionPrice3, values.sharePrice);

        return values;
    }
    
    private decimal? percentIncome(decimal? price, decimal sharePrice)
    {
        if (price == null || sharePrice == 0)
            return null;

        return Math.Round(price.Value / sharePrice * 100, 2, MidpointRounding.ToEven);
    }
    
    private int? findGreaterThanOrEqualIndex(List<OptionData> options, decimal price)
    {
        for (int i = 0; i < options.Count; i++)
        {
            decimal toCheck = options[i].strikePrice;
            if (toCheck >= price)
            {
                int next = i - 1;
                if (next < 0)
                    return i;
                
                // Checks if the next strike price is closer to the target price
                decimal adjacent = options[next].strikePrice;
                if (Math.Abs(adjacent - price) < Math.Abs(toCheck - price))
                    return next;
                
                return i;
            }
        }

        return null;
    }

    private int? findLesserThanOrEqualIndex(List<OptionData> options, decimal price)
    {
        for (int i = options.Count-1; i >= 0; i--)
        {
            decimal toCheck = options[i].strikePrice;
            if (toCheck <= price)
            {
                int next = i + 1;
                if (next >= options.Count)
                    return i;
                
                // Checks if the next strike price is closer to the target price
                decimal adjacent = options[next].strikePrice;
                if (Math.Abs(adjacent - price) < Math.Abs(toCheck - price))
                    return next;
                
                return i;
            }
        }

        return null;
    }

    private decimal roundPrice(decimal price, bool fractional)
    {
        if (fractional)
        {
            price = price * 2;
            price = Math.Round(price, 0, MidpointRounding.ToEven);
            price = price / 2;
        }
        else
        {
            price = Math.Round(price, 0, MidpointRounding.ToEven);
        }

        return price;
    }
    
    private bool hasFactionalStrikePrice(List<OptionData> options)
    {
        return options.Any(x => x.strikePrice != Math.Truncate(x.strikePrice));
    }

    private OptionValues calculateBasis(OptionData option)
    {
        OptionValues result = new();
        result.symbol = option.symbol;
        result.sharePrice = option.sharePrice;
        result.expirationDate = option.expirationDate;
        result.putCallRatio = option.putCallRatio;
        result.beta = option.beta;

        result.options = (int)(investmentAmount / option.sharePrice / 100);
        result.shares = result.options * 100;
        result.costBasis = result.sharePrice * result.sharePrice / 100;

        return result;
    }
}