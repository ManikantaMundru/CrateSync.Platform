namespace CrateSync.Platform.Inventory.Domain.ValueObjects;

public readonly record struct CrateQuantity
{
    public CrateQuantity(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be greater than zero.");
        Value = value;
    }

    public int Value { get; }
}
