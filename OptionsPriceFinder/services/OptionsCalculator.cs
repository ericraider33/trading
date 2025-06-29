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
    
    public OptionValues? calculateOption(string symbol, DateTime expirationDate, List<OptionData> options)
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
       
        int? index1 = findMinIndex(toReview, precent1);
        int? index3 = findMaxIndex(toReview, precent3);
        int? index2 = null;
        
        if (index3 != null && index1 != null)
        {
            if (index3 > index1+1)
                index2 = (index1 + index3) / 2;
        }
        else
        {
            index2 = findMinIndex(toReview, precent2);
        }
        
        OptionData? option1 = index1 != null ? toReview[index1.Value] : null;
        OptionData? option2 = index2 != null ? toReview[index2.Value] : null;
        OptionData? option3 = index3 != null ? toReview[index3.Value] : null;
        
        values.strikePrice1 = option1?.strikePrice; 
        values.strikePrice2 = option2?.strikePrice;
        values.strikePrice3 = option3?.strikePrice;
        
        values.callPrice1 = option1?.callLastPrice;
        values.callPrice2 = option2?.callLastPrice;
        values.callPrice3 = option3?.callLastPrice;

        values.incomePercent1 = percentIncome(values.callPrice1, values.sharePrice);
        values.incomePercent2 = percentIncome(values.callPrice2, values.sharePrice);
        values.incomePercent3 = percentIncome(values.callPrice3, values.sharePrice);

        return values;
    }
    
    private decimal? percentIncome(decimal? price, decimal sharePrice)
    {
        if (price == null || sharePrice == 0)
            return null;

        return Math.Round(price.Value / sharePrice * 100, 2, MidpointRounding.ToEven);
    }
    
    private int? findMinIndex(List<OptionData> options, decimal price)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].strikePrice >= price)
                return i;
        }

        return null;
    }

    private int? findMaxIndex(List<OptionData> options, decimal price)
    {
        for (int i = options.Count-1; i >= 0; i--)
        {
            if (options[i].strikePrice <= price)
                return i;
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

        result.options = (int)(investmentAmount / option.sharePrice / 100);
        result.shares = result.options * 100;
        result.costBasis = result.sharePrice * result.sharePrice / 100;

        return result;
    }
}