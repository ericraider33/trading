namespace algorithm.model;

public class History
{
    public string symbol { get; set; } = "";
    public DateTime timestamp { get; set; }
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public long volume { get; set; }
    
    public decimal? movingAverage21 { get; set; }
    public decimal? movingAverage49 { get; set; }
    public decimal? movingAverage203 { get; set; }
    
    public bool isAboveMovingAverage21And49()
    {
        return movingAverage21.HasValue 
            && movingAverage49.HasValue 
            && open >= movingAverage21.Value 
            && open >= movingAverage49.Value;
    }
    
    public bool isBelowMovingAverage21And49()
    {
        return movingAverage21.HasValue 
            && movingAverage49.HasValue 
            && open <= movingAverage21.Value 
            && open <= movingAverage49.Value;
    }
}