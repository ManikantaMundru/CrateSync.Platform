namespace CrateSync.Platform.TradingPartners.Domain.Suppliers;

public readonly record struct SupplierId(Guid Value)
{
    public static SupplierId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
