using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.Orders.SearchOrders;

public sealed record SalesOrderSummaryResponse(
    Guid Id,
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    SalesOrderStatus Status,
    decimal TotalAmount,
    string? Currency,
    string? ReservationFailureReason,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
