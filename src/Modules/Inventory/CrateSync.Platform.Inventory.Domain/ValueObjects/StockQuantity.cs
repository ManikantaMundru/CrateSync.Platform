namespace CrateSync.Platform.Inventory.Domain.ValueObjects;

public readonly record struct StockQuantity
{
    public StockQuantity(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Stock quantity cannot be negative.");

        Value = value;
    }

    public int Value { get; }

    public static StockQuantity Zero => new(0);

    public override string ToString() => Value.ToString();
}
