using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.InboundLogistics.Application.Abstractions;
using CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;
using CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Dapper;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Read;

internal sealed class InboundLogisticsReadService(ISqlConnectionFactory connectionFactory) : IInboundLogisticsReadService
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<ShipmentResponse?> GetByIdAsync(
        Guid tenantId,
        Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                SupplierId,
                WarehouseId,
                TrackingNumber,
                OriginCountry,
                CASE Status
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Shipped'
                    WHEN 3 THEN 'Arrived'
                    WHEN 4 THEN 'Receiving'
                    WHEN 5 THEN 'PartiallyReceived'
                    WHEN 6 THEN 'Received'
                    WHEN 7 THEN 'Cancelled'
                END AS Status,
                ExpectedArrivalAtUtc,
                ShippedAtUtc,
                ArrivedAtUtc,
                ReceivedAtUtc,
                CancelledAtUtc,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM logistics.InboundShipments
            WHERE Id = @ShipmentId
              AND TenantId = @TenantId;

            SELECT
                Id,
                ProductId,
                ProductVarietyId,
                BatchCode,
                ExpectedQuantity,
                ReceivedQuantity,
                PackSize AS PackSizeKg,
                UnitCost,
                Currency
            FROM logistics.InboundShipmentLines
            WHERE ShipmentId = @ShipmentId
            ORDER BY Id;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(
                sql,
                new
                {
                    ShipmentId = shipmentId,
                    TenantId = tenantId
                },
                cancellationToken: cancellationToken));

        var header = await result.ReadSingleOrDefaultAsync<ShipmentHeader>();

        if (header is null)
            return null;

        var lines = (await result.ReadAsync<ShipmentLineResponse>()).AsList();

        return new ShipmentResponse(
            header.Id,
            header.SupplierId,
            header.WarehouseId,
            header.TrackingNumber,
            header.OriginCountry,
            header.Status,
            header.ExpectedArrivalAtUtc,
            header.ShippedAtUtc,
            header.ArrivedAtUtc,
            header.ReceivedAtUtc,
            header.CancelledAtUtc,
            header.CreatedAtUtc,
            header.UpdatedAtUtc,
            lines);
    }

    public async Task<IReadOnlyCollection<ShipmentSummaryResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        ShipmentStatus? status,
        Guid? supplierId,
        Guid? warehouseId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                s.Id,
                s.SupplierId,
                s.WarehouseId,
                s.TrackingNumber,
                s.OriginCountry,
                CASE s.Status
                    WHEN 1 THEN 'Draft'
                    WHEN 2 THEN 'Shipped'
                    WHEN 3 THEN 'Arrived'
                    WHEN 4 THEN 'Receiving'
                    WHEN 5 THEN 'PartiallyReceived'
                    WHEN 6 THEN 'Received'
                    WHEN 7 THEN 'Cancelled'
                END AS Status,
                s.ExpectedArrivalAtUtc,
                COALESCE(SUM(l.ExpectedQuantity), 0) AS TotalExpectedCrates,
                COALESCE(SUM(l.ReceivedQuantity), 0) AS TotalReceivedCrates
            FROM logistics.InboundShipments s
            LEFT JOIN logistics.InboundShipmentLines l
                ON l.ShipmentId = s.Id
            WHERE s.TenantId = @TenantId
              AND (@SearchTerm IS NULL
                   OR s.TrackingNumber LIKE '%' + @SearchTerm + '%')
              AND (@Status IS NULL OR s.Status = @Status)
              AND (@SupplierId IS NULL OR s.SupplierId = @SupplierId)
              AND (@WarehouseId IS NULL OR s.WarehouseId = @WarehouseId)
            GROUP BY
                s.Id,
                s.SupplierId,
                s.WarehouseId,
                s.TrackingNumber,
                s.OriginCountry,
                s.Status,
                s.ExpectedArrivalAtUtc
            ORDER BY s.ExpectedArrivalAtUtc DESC;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var shipments = await connection.QueryAsync<ShipmentSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm)
                        ? null
                        : searchTerm.Trim(),
                    Status = (int?)status,
                    SupplierId = supplierId,
                    WarehouseId = warehouseId
                },
                cancellationToken: cancellationToken));

        return shipments.AsList();
    }

    private sealed record ShipmentHeader(
        Guid Id,
        Guid SupplierId,
        Guid WarehouseId,
        string TrackingNumber,
        string OriginCountry,
        string Status,
        DateTimeOffset ExpectedArrivalAtUtc,
        DateTimeOffset? ShippedAtUtc,
        DateTimeOffset? ArrivedAtUtc,
        DateTimeOffset? ReceivedAtUtc,
        DateTimeOffset? CancelledAtUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
