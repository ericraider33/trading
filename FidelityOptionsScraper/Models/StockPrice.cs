using System;
using System.Collections.Generic;

namespace FidelityOptionsScraper.Models
{
    public class StockPrice
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public DateTime RetrievalTime { get; set; }
    }
}
