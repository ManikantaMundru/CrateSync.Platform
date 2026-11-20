namespace CrateSync.Platform.Sales.Domain.Orders;

public readonly record struct SalesOrderId
{
    public SalesOrderId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Sales order ID cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static SalesOrderId New() => new(Guid.NewGuid());
    public static SalesOrderId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
