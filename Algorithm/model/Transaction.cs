namespace algorithm.model;

public class Transaction : ICloneable
{
    public string symbol { get; set; } = "";
    public decimal quantity { get; set; }
    public decimal price { get; set;}
    public DateTime timestamp { get; set; }
    public TransactionTypeEnum type { get; set; }
    
    public object Clone()
    {
        return MemberwiseClone();
    }
}