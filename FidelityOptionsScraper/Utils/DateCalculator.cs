using pnyx.net.util.dates;

namespace FidelityOptionsScraper.Utils;

public static class DateCalculator
{
    /// <summary>
    /// Gets the next Friday from a given date
    /// </summary>
    public static LocalDay getNextFriday(LocalDay fromDate)
    {
        // Calculate days until next Friday
        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)fromDate.local.DayOfWeek + 7) % 7;
            
        // If today is Friday, get next Friday (add 7 days)
        if (daysUntilFriday == 0)
            daysUntilFriday = 7;
                
        return fromDate.addDays(daysUntilFriday);
    }
}