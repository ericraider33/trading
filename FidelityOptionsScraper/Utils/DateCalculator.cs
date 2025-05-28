using System;

namespace FidelityOptionsScraper.Utils
{
    public static class DateCalculator
    {
        /// <summary>
        /// Gets the next Friday from a given date
        /// </summary>
        /// <param name="fromDate">The starting date</param>
        /// <returns>The next Friday date (or Friday of next week if today is Friday)</returns>
        public static DateTime GetNextFriday(DateTime fromDate)
        {
            // Calculate days until next Friday
            int daysUntilFriday = ((int)DayOfWeek.Friday - (int)fromDate.DayOfWeek + 7) % 7;
            
            // If today is Friday, get next Friday (add 7 days)
            if (daysUntilFriday == 0)
                daysUntilFriday = 7;
                
            return fromDate.AddDays(daysUntilFriday);
        }
    }
}
