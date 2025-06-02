namespace OptionsPriceFinder.Utils;

public static class DateCalculator
{
    public static DateTime getNextFriday(DateTime fromDate)
    {
        // Calculate days until next Friday
        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)fromDate.DayOfWeek + 7) % 7;
            
        // If today is Friday, get next Friday (add 7 days)
        if (daysUntilFriday == 0)
            daysUntilFriday = 7;
                
        return fromDate.AddDays(daysUntilFriday);
    }
}