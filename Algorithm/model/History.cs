namespace algorithm.model;

public class History
{
    public string symbol { get; set; } = "";
    public DateTime timestamp { get; set; }
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public int volume { get; set; }
}