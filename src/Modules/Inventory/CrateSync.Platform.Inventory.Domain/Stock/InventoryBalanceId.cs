namespace CrateSync.Platform.Inventory.Domain.Stock;

public readonly record struct InventoryBalanceId
{
    public InventoryBalanceId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Inventory balance ID cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static InventoryBalanceId New() => new(Guid.NewGuid());
    public static InventoryBalanceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
