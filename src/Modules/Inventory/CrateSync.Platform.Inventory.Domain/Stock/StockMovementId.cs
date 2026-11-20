namespace CrateSync.Platform.Inventory.Domain.Stock;

public readonly record struct StockMovementId
{
    public StockMovementId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Stock movement ID cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static StockMovementId New() => new(Guid.NewGuid());
    public static StockMovementId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
