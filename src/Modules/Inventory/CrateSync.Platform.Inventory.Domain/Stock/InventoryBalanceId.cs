namespace CrateSync.Platform.Inventory.Domain.Stock;

public readonly record struct InventoryBalanceId(Guid Value)
{
    public static InventoryBalanceId New() => new(Guid.NewGuid());

    public static InventoryBalanceId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Inventory balance ID cannot be empty.", nameof(value));

        return new InventoryBalanceId(value);
    }

    public override string ToString() => Value.ToString();
}
