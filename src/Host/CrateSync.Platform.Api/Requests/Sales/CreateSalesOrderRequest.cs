namespace CrateSync.Platform.Api.Requests.Sales;

public sealed record CreateSalesOrderRequest(
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? Province,
    string? PostalCode,
    string Country);
