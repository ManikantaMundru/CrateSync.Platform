namespace CrateSync.Platform.Sales.Contracts;

public sealed record SalesOrderLineContract(
    Guid SalesOrderLineId,
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency);
