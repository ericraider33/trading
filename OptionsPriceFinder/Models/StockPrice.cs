namespace OptionsPriceFinder.Models
{
    public class StockPrice
    {
        public string Symbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime RetrievalTime { get; set; }
    }
}
