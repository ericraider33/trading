using System;
using System.Collections.Generic;

namespace FidelityOptionsScraper.Models
{
    public class OptionResult
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public string FridayDate { get; set; } = string.Empty;
        public decimal? CallOption1Percent { get; set; }
        public decimal? CallOption2Percent { get; set; }
        public decimal? CallOption3Percent { get; set; }
    }
}
