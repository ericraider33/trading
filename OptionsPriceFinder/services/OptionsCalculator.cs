using FidelityOptionsScraper.Models;
using OptionsPriceFinder.model;
using OptionsPriceFinder.utils;

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
    
    public List<OptionSpread>? calculatePutSpread(string symbol, DateTime expirationDate, List<OptionData> options, int limit = 1)
    {
        // Limits to OTM put options (aka with strike price below share price)
        List<OptionData> toReview = options
            .Where(x => x.symbol == symbol && x.expirationDate == expirationDate && x.strikePrice < x.sharePrice)
            .OrderByDescending(x => x.strikePrice)
            .ToList();

        if (toReview.Count < 2)
            return null;

        OptionValues values = calculateBasis(toReview[0]);
        if (values.shares == 0)
            return null;

        OptionSpread spreadBase = OptionSpread.fromOptionValues(values);
        List<OptionSpread> spreads = new();

        for (int i = 0; i < toReview.Count - 1; i++)
        {
            OptionData optionSell = toReview[i];
            for (int j = i + 1; j < toReview.Count; j++)
            {
                OptionSpread spread = spreadBase.cloneAs();
                OptionData optionBuy = toReview[j];
                
                spread.strikePriceSell = optionSell.strikePrice;
                spread.optionPriceSell = optionSell.getPutPriceBestGuess() ?? -1m;
                spread.strikePriceBuy = optionBuy.strikePrice;
                spread.optionPriceBuy = optionBuy.getPutPriceBestGuess() ?? -1m;
                
                if (spread.optionPriceSell <= 0 || spread.optionPriceBuy <= 0)
                    continue;

                spread.maximumGain = spread.optionPriceSell - spread.optionPriceBuy;
                spread.maximumLoss = spread.strikePriceSell - spread.strikePriceBuy - spread.maximumGain;
                 
                if (spread.maximumLoss <= 0 || spread.maximumGain <= 0)
                    continue;
                
                spread.maximumRatio = Math.Round(spread.maximumLoss / spread.maximumGain, 4, MidpointRounding.ToEven);

                if (!optionSell.putDelta.HasValue || !optionBuy.putDelta.HasValue)
                    continue;
                
                spread.deltaSell = optionSell.putDelta.Value;
                spread.deltaBuy = optionBuy.putDelta.Value;
                
                decimal? spreadValue = calculateSpreadValue(spread);
                if (spreadValue == null)
                    continue;

                spread.spreadValue = spreadValue.Value;
                
                spreads.Add(spread);                
            }
        }

        if (spreads.Count == 0)
            return null;
        
        spreads = spreads.OrderByDescending(x => x.spreadValue).Take(limit).ToList();
        return spreads;
    }
    
    public List<OptionSpread>? calculateCallSpread(string symbol, DateTime expirationDate, List<OptionData> options, int limit = 1)
    {
        // Limits to OTM call options (aka with strike price above share price)
        List<OptionData> toReview = options
            .Where(x => x.symbol == symbol && x.expirationDate == expirationDate && x.strikePrice > x.sharePrice)
            .OrderBy(x => x.strikePrice)
            .ToList();

        if (toReview.Count < 2)
            return null;

        OptionValues values = calculateBasis(toReview[0]);
        if (values.shares == 0)
            return null;

        OptionSpread spreadBase = OptionSpread.fromOptionValues(values);
        List<OptionSpread> spreads = new();

        for (int i = 0; i < toReview.Count - 1; i++)
        {
            OptionData optionSell = toReview[i];
            for (int j = i + 1; j < toReview.Count; j++)
            {
                OptionSpread spread = spreadBase.cloneAs();
                OptionData optionBuy = toReview[j];
                
                spread.strikePriceSell = optionSell.strikePrice;
                spread.optionPriceSell = optionSell.getCallPriceBestGuess() ?? -1m;
                spread.strikePriceBuy = optionBuy.strikePrice;
                spread.optionPriceBuy = optionBuy.getCallPriceBestGuess() ?? -1m;
                
                if (spread.optionPriceSell <= 0 || spread.optionPriceBuy <= 0)
                    continue;

                spread.maximumGain = spread.optionPriceSell - spread.optionPriceBuy;
                spread.maximumLoss = spread.strikePriceBuy - spread.strikePriceSell - spread.maximumGain;
                 
                if (spread.maximumLoss <= 0 || spread.maximumGain <= 0)
                    continue;
                
                spread.maximumRatio = Math.Round(spread.maximumLoss / spread.maximumGain, 4, MidpointRounding.ToEven);

                if (!optionSell.callDelta.HasValue || !optionBuy.callDelta.HasValue)
                    continue;
                
                spread.deltaSell = optionSell.callDelta.Value;
                spread.deltaBuy = optionBuy.callDelta.Value;
                
                decimal? spreadValue = calculateSpreadValue(spread);
                if (spreadValue == null)
                    continue;

                spread.spreadValue = spreadValue.Value;
                
                spreads.Add(spread);                
            }
        }

        if (spreads.Count == 0)
            return null;
        
        spreads = spreads.OrderByDescending(x => x.spreadValue).Take(limit).ToList();
        return spreads;
    }
    
    private decimal? calculateSpreadValue(OptionSpread spread)
    {
        // Chance that maximum profit is achieved because the spread expires worthless
        decimal chanceOfExpire = 1 - Math.Abs(spread.deltaSell);
        
        // Chance that maximum loss is achieved because the spread is assigned
        decimal chanceOfAssignment = Math.Abs(spread.deltaBuy);
        
        // Chance that prices is between the two strike prices at expiration
        decimal between = 1 - chanceOfExpire - chanceOfAssignment;
        
        // Average between maximum gain and loss
        decimal averageBetween = MathUtil.average(spread.maximumGain, -1 * spread.maximumLoss);
        
        // Using the delta as a measure of risk, this is probabilistic, cost-weighted value of the spread
        decimal maxGainValue = spread.maximumGain * chanceOfExpire;
        decimal maxLossValue = -1 * spread.maximumLoss * chanceOfAssignment;
        decimal betweenValue = between * averageBetween;
        
        decimal spreadValue = maxGainValue + maxLossValue + betweenValue;

        if (between <= 0 || chanceOfExpire == 1m || chanceOfAssignment == 0m)
            return null;
        
        return spreadValue;
    }
    
    /// <summary>
    /// Returns the next 5 weekly expiration dates starting from the given expiration date.
    /// </summary>
    private List<DateTime> getExpirationDates(DateTime expirationDate)
    {
        List<DateTime> dates = new();
        for (int i = 0; i < 5; i++)
        {
            dates.Add(expirationDate.AddDays(i * 7));
        }
        return dates;
    }
    
    private OptionDateGain buildDateGain(OptionData option, List<DateTime> dates)
    {
        OptionDateGain result = new();
        result.symbol = option.symbol;
        result.sharePrice = option.sharePrice;
        result.putCallRatio = option.putCallRatio;
        result.beta = option.beta;

        foreach (DateTime date in dates)
        {
            result.Values.Add(new OptionDateGain.InfoForDate
            {
                expirationDate = date
            });
        }

        return result;
    }
    
    public OptionDateGain? calculateCallDateGain(string symbol, DateTime expirationDate, List<OptionData> options)
    {
        List<DateTime> dates = getExpirationDates(expirationDate);
        List<OptionData> toReview = options
            .Where(x => x.symbol == symbol && dates.Contains(x.expirationDate))
            .OrderBy(x => x.expirationDate)
            .ThenBy(x => x.strikePrice)
            .ToList();

        int matchingDates = toReview.Select(x => x.expirationDate).Distinct().Count();
        if (toReview.Count == 0 || matchingDates != dates.Count)
            return null;

        OptionDateGain gain = buildDateGain(options[0], dates);
        if (gain.sharePrice == 0)
            return null;

        DateTime lastDate = dates.Last();
        List<OptionData> toCheck = toReview.Where(x => x.expirationDate == lastDate).ToList();
        
        decimal precent3 = roundPrice(1.03m * gain.sharePrice, false);
        int? index3 = findLesserThanOrEqualIndex(toCheck, precent3);
        if (index3 == null)
            return null;

        // Use the strike price from the last date to find matching strike prices across all dates
        gain.strikePrice = toCheck[index3.Value].strikePrice;

        // Verifies that all dates have the same strike price
        List<OptionData> strikeOptions = toReview.Where(x => x.strikePrice == gain.strikePrice).ToList();
        if (strikeOptions.Count != dates.Count)
            return null;
        
        for (int i = 0; i < dates.Count; i++)
        {
            DateTime date = dates[i];
            OptionData option = strikeOptions.First(x => x.expirationDate == date);
            decimal? optionPrice = option.getCallPriceBestGuess();
            if (optionPrice == null)
                return null;
            
            decimal? incomePercent = percentIncome(optionPrice, gain.sharePrice);
            if (incomePercent == null)
                return null;

            gain.Values[i].optionPrice = optionPrice;
            gain.Values[i].incomePercent = incomePercent;
        }
        
        return gain;
    }
    
}