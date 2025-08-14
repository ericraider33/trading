namespace algorithm.model;

public class Position : ICloneable
{
    public const string CASH_SYMBOL = "CASH";
    
    public string symbol { get; set; } = "";
    public decimal quantity { get; set; }
    public PositionTypeEnum type { get; set; }
    
    public object Clone()
    {
        Position result = (Position)MemberwiseClone();
        return result;
    }
    
    public decimal valueForPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");
        
        return quantity * price;
    }
}