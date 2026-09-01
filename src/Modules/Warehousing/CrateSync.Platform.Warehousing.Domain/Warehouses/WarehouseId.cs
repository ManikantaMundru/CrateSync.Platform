namespace CrateSync.Platform.Warehousing.Domain.Warehouses;

public readonly record struct WarehouseId(Guid Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
