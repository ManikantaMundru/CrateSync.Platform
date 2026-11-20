namespace CrateSync.Platform.Sales.Contracts;

public sealed record SalesOrderContract(
    Guid SalesOrderId,
    Guid TenantId,
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    string Status,
    decimal TotalAmount,
    string Currency,
    IReadOnlyCollection<SalesOrderLineContract> Lines);
