namespace CrateSync.Platform.TradingPartners.Domain.Customers;

public readonly record struct CustomerId(Guid Value)
{
    public static CustomerId New() => new(Guid.NewGuid());

    public static CustomerId From(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Customer ID cannot be empty.", nameof(value));

        return new CustomerId(value);
    }

    public override string ToString() => Value.ToString();
}
