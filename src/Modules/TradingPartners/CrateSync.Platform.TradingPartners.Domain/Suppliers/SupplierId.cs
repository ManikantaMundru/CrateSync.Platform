namespace CrateSync.Platform.TradingPartners.Domain.Suppliers;

public readonly record struct SupplierId(Guid Value)
{
    public static SupplierId New() => new(Guid.NewGuid());

    public static SupplierId From(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Supplier ID cannot be empty.", nameof(value));

        return new SupplierId(value);
    }

    public override string ToString() => Value.ToString();
}
