using System;
using System.Collections.Generic;

namespace FidelityOptionsScraper.Models
{
    public class OptionData
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal StrikePrice { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal? CallPrice { get; set; }
    }
}
