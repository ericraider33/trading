namespace OptionsPriceFinder.Models
{
    public class OptionResult
    {
        public string Symbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public string FridayDate { get; set; }
        public decimal? CallOption1Percent { get; set; }
        public decimal? CallOption2Percent { get; set; }
        public decimal? CallOption3Percent { get; set; }
    }
}
