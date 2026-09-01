namespace CrateSync.Platform.Sales.Domain.Orders;

public readonly record struct SalesOrderLineId(Guid Value)
{
    public static SalesOrderLineId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
