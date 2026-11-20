namespace CrateSync.Platform.Api.Requests.Sales;

public sealed record UpdateSalesOrderLineRequest(
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency);
