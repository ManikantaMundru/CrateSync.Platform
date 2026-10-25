namespace CrateSync.Platform.Warehousing.Domain.Warehouses;

public readonly record struct WarehouseId(Guid Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());

    public static WarehouseId From(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Warehouse ID cannot be empty.", nameof(value));

        return new WarehouseId(value);
    }

    public override string ToString() => Value.ToString();
}
