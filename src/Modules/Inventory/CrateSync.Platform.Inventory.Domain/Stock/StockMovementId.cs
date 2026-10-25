namespace CrateSync.Platform.Inventory.Domain.Stock;

public readonly record struct StockMovementId(Guid Value)
{
    public static StockMovementId New() => new(Guid.NewGuid());

    public static StockMovementId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Stock movement ID cannot be empty.", nameof(value));

        return new StockMovementId(value);
    }

    public override string ToString() => Value.ToString();
}
