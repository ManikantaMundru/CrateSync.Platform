namespace CrateSync.Platform.IntegrationContracts.Sales;

public sealed record SalesOrderFulfilledLineIntegrationContract(
    Guid SalesOrderLineId,
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency);
