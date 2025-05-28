namespace OptionsPriceFinder.Models
{
    public class OptionData
    {
        public string Symbol { get; set; }
        public decimal StrikePrice { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal? CallPrice { get; set; }
    }
}
