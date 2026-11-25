namespace CrateSync.Platform.Sales.Application.Orders.GetOrder;

public sealed record SalesOrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    string Status,
    string? ReservationFailureReason,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? Province,
    string? PostalCode,
    string Country,
    decimal TotalAmount,
    string? Currency,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<SalesOrderLineResponse> Lines);

public sealed record SalesOrderLineResponse(
    Guid Id,
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency,
    decimal LineTotal,
    string OriginCountry = "ZZ", Guid BatchId = default, string? BatchCode = null, int? FruitsPerPack = null);
