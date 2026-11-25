using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Application.Orders.GetOrder;
using CrateSync.Platform.Sales.Application.Orders.SearchOrders;
using CrateSync.Platform.Sales.Domain.Orders;
using Dapper;

namespace CrateSync.Platform.Sales.Infrastructure.Read;

internal sealed class SalesOrderReadService(ISqlConnectionFactory connectionFactory) : ISalesOrderReadService
{
    public async Task<SalesOrderResponse?> GetAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        const string orderSql = """
            SELECT
                Id,
                CustomerId,
                WarehouseId,
                OrderNumber,
                CASE Status
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Confirmed'
                    WHEN 3 THEN 'Allocated'
                    WHEN 4 THEN 'Dispatched'
                    WHEN 5 THEN 'Fulfilled'
                    WHEN 6 THEN 'Cancelled'
                END AS Status,
                ReservationFailureReason,
                DeliveryAddressLine1 AS AddressLine1,
                DeliveryAddressLine2 AS AddressLine2,
                DeliveryCity AS City,
                DeliveryProvince AS Province,
                DeliveryPostalCode AS PostalCode,
                DeliveryCountry AS Country,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM sales.SalesOrders
            WHERE TenantId = @TenantId
              AND Id = @SalesOrderId;
            """;

        const string linesSql = """
            SELECT
                Id,
                ProductVarietyId,
                Quantity,
                PackSize,
                UnitPrice,
                Currency,
                Quantity * UnitPrice AS LineTotal,
                OriginCountry
            FROM sales.SalesOrderLines
            WHERE SalesOrderId = @SalesOrderId
            ORDER BY Id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var order = await connection.QuerySingleOrDefaultAsync<OrderRow>(
            new CommandDefinition(
                orderSql,
                new { TenantId = tenantId, SalesOrderId = salesOrderId },
                cancellationToken: cancellationToken));

        if (order is null) return null;

        var lines = (await connection.QueryAsync<SalesOrderLineResponse>(
            new CommandDefinition(
                linesSql,
                new { SalesOrderId = salesOrderId },
                cancellationToken: cancellationToken))).AsList();

        return new SalesOrderResponse(
            order.Id,
            order.CustomerId,
            order.WarehouseId,
            order.OrderNumber,
            order.Status,
            order.ReservationFailureReason,
            order.AddressLine1,
            order.AddressLine2,
            order.City,
            order.Province,
            order.PostalCode,
            order.Country,
            lines.Sum(x => x.LineTotal),
            lines.Select(x => x.Currency).FirstOrDefault(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            lines);
    }

    public async Task<IReadOnlyCollection<SalesOrderSummaryResponse>> SearchAsync(
        Guid tenantId,
        SalesOrderStatus? status,
        Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                o.Id,
                o.CustomerId,
                o.WarehouseId,
                o.OrderNumber,
                o.Status,
                COALESCE(SUM(l.Quantity * l.UnitPrice), 0) AS TotalAmount,
                MAX(l.Currency) AS Currency,
                o.ReservationFailureReason,
                o.CreatedAtUtc,
                o.UpdatedAtUtc
            FROM sales.SalesOrders o
            LEFT JOIN sales.SalesOrderLines l ON l.SalesOrderId = o.Id
            WHERE o.TenantId = @TenantId
              AND (@Status IS NULL OR o.Status = @Status)
              AND (@CustomerId IS NULL OR o.CustomerId = @CustomerId)
            GROUP BY
                o.Id,
                o.CustomerId,
                o.WarehouseId,
                o.OrderNumber,
                o.Status,
                o.ReservationFailureReason,
                o.CreatedAtUtc,
                o.UpdatedAtUtc
            ORDER BY o.CreatedAtUtc DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<SalesOrderSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    Status = status is null ? null : (int?)status.Value,
                    CustomerId = customerId
                },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    private sealed record OrderRow(
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
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
