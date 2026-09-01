namespace CrateSync.Platform.Sales.Domain.Orders;

public readonly record struct SalesOrderId(Guid Value)
{
    public static SalesOrderId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
