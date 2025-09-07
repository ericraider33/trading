using OptionsPriceFinder.model;
using pnyx.net.api;

namespace OptionsPriceFinder.utils;

public class OptionDateGainConverter : IObjectConverterFromNameValuePair
{
    private readonly int dateCount;

    public OptionDateGainConverter(int dateCount)
    {
        this.dateCount = dateCount;
    }

    public List<string> getHeaders()
    {
        List<string> headers = new();
        headers.Add(nameof(OptionDateGain.symbol));
        headers.Add(nameof(OptionDateGain.sharePrice));
        headers.Add(nameof(OptionDateGain.putCallRatio));
        headers.Add(nameof(OptionDateGain.beta));
        headers.Add(nameof(OptionDateGain.strikePrice));
        
        for (int i = 1; i <= dateCount; i++)
        {
            headers.Add($"expirationDate{i}");
            headers.Add($"optionPrice{i}");
            headers.Add($"incomePercent{i}");
        }

        return headers;
    }

    public IDictionary<string, object> objectToNameValuePair(object obj)
    {
        OptionDateGain optionDateGain = (OptionDateGain)obj;
        Dictionary<string, object> result = new();
        result.Add(nameof(OptionDateGain.symbol), optionDateGain.symbol);
        result.Add(nameof(OptionDateGain.sharePrice), optionDateGain.sharePrice);
        result.Add(nameof(OptionDateGain.putCallRatio), optionDateGain.putCallRatio?.ToString() ?? "");
        result.Add(nameof(OptionDateGain.beta), optionDateGain.beta?.ToString() ?? "");
        result.Add(nameof(OptionDateGain.strikePrice), optionDateGain.strikePrice);
        
        for (int i = 1; i <= dateCount; i++)
        {
            OptionDateGain.InfoForDate? value = null;
            if (i <= optionDateGain.Values.Count)
                value = optionDateGain.Values[i-1];
            
            result.Add($"expirationDate{i}", value?.expirationDate.ToString("yyyy-MM-dd") ?? "");
            result.Add($"optionPrice{i}", value?.optionPrice?.ToString() ?? "");
            result.Add($"incomePercent{i}", value?.incomePercent?.ToString() ?? "");
        }
        
        return result;
    }

    public object nameValuePairToObject(IDictionary<string, object> row)
    {
        OptionDateGain optionDateGain = new();
        optionDateGain.symbol = row[nameof(OptionDateGain.symbol)].ToString();
        optionDateGain.sharePrice = decimal.Parse(row[nameof(OptionDateGain.sharePrice)].ToString());
        
        string x = row[nameof(OptionDateGain.putCallRatio)].ToString() ?? "";
        optionDateGain.putCallRatio = string.IsNullOrEmpty(x) ? null : decimal.Parse(x);
        
        x = row[nameof(OptionDateGain.beta)].ToString() ?? "";
        optionDateGain.beta = string.IsNullOrEmpty(x) ? null : decimal.Parse(x);

        optionDateGain.strikePrice = decimal.Parse(row[nameof(OptionDateGain.strikePrice)].ToString());

        for (int i = 1; i <= dateCount; i++)
        {
            OptionDateGain.InfoForDate info = new();
            if (row.ContainsKey($"expirationDate{i}"))
            {
                info.expirationDate = DateTime.Parse(row[$"expirationDate{i}"].ToString());
            
                x = row[$"optionPrice{i}"].ToString() ?? "";
                info.optionPrice = string.IsNullOrEmpty(x) ? null : decimal.Parse(x);
            
                x = row[$"incomePercent{i}"].ToString() ?? "";
                info.incomePercent = string.IsNullOrEmpty(x) ? null : decimal.Parse(x);
            }
            
            optionDateGain.Values.Add(info);
            i++;
        }

        return optionDateGain;
    }
}