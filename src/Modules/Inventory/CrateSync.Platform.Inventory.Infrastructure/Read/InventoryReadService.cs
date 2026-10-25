using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using Dapper;

namespace CrateSync.Platform.Inventory.Infrastructure.Read;

internal sealed class InventoryReadService(
    ISqlConnectionFactory connectionFactory)
    : IInventoryReadService
{
    public async Task<StockResponse?> GetStockAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                Id,
                WarehouseId,
                ProductVarietyId,
                OnHand,
                Reserved,
                OnHand - Reserved AS Available,
                UpdatedAtUtc
            FROM inventory.InventoryBalances
            WHERE TenantId = @TenantId
              AND WarehouseId = @WarehouseId
              AND ProductVarietyId = @ProductVarietyId;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<StockResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    WarehouseId = warehouseId,
                    ProductVarietyId = productVarietyId
                },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<StockSummaryResponse>> SearchStockAsync(
        Guid tenantId,
        Guid? warehouseId,
        Guid? productVarietyId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                Id,
                WarehouseId,
                ProductVarietyId,
                OnHand,
                Reserved,
                OnHand - Reserved AS Available,
                UpdatedAtUtc
            FROM inventory.InventoryBalances
            WHERE TenantId = @TenantId
              AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
              AND (@ProductVarietyId IS NULL OR ProductVarietyId = @ProductVarietyId)
            ORDER BY UpdatedAtUtc DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var results =
            await connection.QueryAsync<StockSummaryResponse>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        TenantId = tenantId,
                        WarehouseId = warehouseId,
                        ProductVarietyId = productVarietyId
                    },
                    cancellationToken: cancellationToken));

        return results.AsList();
    }

    public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                m.Id,
                CASE m.Type
                    WHEN 1 THEN 'Received'
                    WHEN 2 THEN 'Reserved'
                    WHEN 3 THEN 'ReservationReleased'
                    WHEN 4 THEN 'Dispatched'
                    WHEN 5 THEN 'Adjusted'
                END AS Type,
                m.OnHandChange,
                m.ReservedChange,
                m.OnHandAfter,
                m.ReservedAfter,
                m.ReferenceType,
                m.ReferenceId,
                m.Reason,
                m.OccurredAtUtc
            FROM inventory.StockMovements m
            INNER JOIN inventory.InventoryBalances b
                ON b.Id = m.InventoryBalanceId
            WHERE b.TenantId = @TenantId
              AND b.WarehouseId = @WarehouseId
              AND b.ProductVarietyId = @ProductVarietyId
            ORDER BY m.OccurredAtUtc DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(cancellationToken);

        var results =
            await connection.QueryAsync<StockMovementResponse>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        TenantId = tenantId,
                        WarehouseId = warehouseId,
                        ProductVarietyId = productVarietyId
                    },
                    cancellationToken: cancellationToken));

        return results.AsList();
    }
}
