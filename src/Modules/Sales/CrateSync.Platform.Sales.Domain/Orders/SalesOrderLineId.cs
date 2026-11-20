namespace CrateSync.Platform.Sales.Domain.Orders;

public readonly record struct SalesOrderLineId
{
    public SalesOrderLineId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Sales order line ID cannot be empty.", nameof(value));
        Value = value;
    }

    public Guid Value { get; }
    public static SalesOrderLineId New() => new(Guid.NewGuid());
    public static SalesOrderLineId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
