namespace algorithm.model;

public class PositionInvestment : ICloneable
{
    public Position position { get; set; } = null!;
    public Position cash { get; set; } = null!;
    public List<Transaction> transactions { get; set; } = new ();
    
    public static PositionInvestment fromCash(string symbol, decimal amount)
    {
        return new PositionInvestment
        {
            position = new Position { symbol = symbol, type = PositionTypeEnum.Long },
            cash = new Position { symbol = Position.CASH_SYMBOL, type = PositionTypeEnum.Long, quantity = amount },
        };
    }
    
    public object Clone()
    {
        PositionInvestment clone = (PositionInvestment)this.MemberwiseClone();
        clone.position = (Position)this.position.Clone();
        clone.cash = (Position)this.cash.Clone();
        clone.transactions = this.transactions.Select(t => (Transaction)t.Clone()).ToList();
        return clone;
    }

    public void sell(int toSellQuantity, decimal price, DateTime timestamp)
    {
        if (toSellQuantity <= 0)
            throw new ArgumentException("Quantity cannot be negative");

        if (toSellQuantity > position.quantity)
            throw new ArgumentException("Quantity cannot be greater than the current position");
        
        decimal toSellValue = toSellQuantity * price;
        
        position.quantity -= toSellQuantity;
        cash.quantity += toSellValue;
        
        transactions.Add(new Transaction
        {
            symbol = position.symbol,
            quantity = toSellQuantity,
            price = price,
            timestamp = timestamp,
            type = TransactionTypeEnum.Sell
        });
    }
    
    public void buy(int toBuyQuantity, decimal price, DateTime timestamp)
    {
        if (toBuyQuantity <= 0)
            throw new ArgumentException("Quantity cannot be negative");

        decimal toBuyValue = toBuyQuantity * price;
        
        if (toBuyValue > cash.quantity)
            throw new ArgumentException("Not enough cash to buy the position");
        
        position.quantity += toBuyQuantity;
        cash.quantity -= toBuyValue;
        
        transactions.Add(new Transaction
        {
            symbol = position.symbol,
            quantity = toBuyQuantity,
            price = price,
            timestamp = timestamp,
            type = TransactionTypeEnum.Buy
        });
    }
   
    public decimal valueForPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");
        
        return position.valueForPrice(price) + cash.quantity;
    }
    
    public decimal gains(PositionInvestment initialInvestment, decimal openPrice, decimal closePrice)
    {
        if (initialInvestment == null)
            throw new ArgumentNullException(nameof(initialInvestment));
        
        if (openPrice < 0 || closePrice < 0)
            throw new ArgumentException("Price cannot be negative");

        decimal initialValue = initialInvestment.valueForPrice(openPrice);
        decimal currentValue = valueForPrice(closePrice);
        
        return currentValue - initialValue;
    }
}