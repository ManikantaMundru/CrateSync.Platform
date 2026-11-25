using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;
using Dapper;

namespace CrateSync.Platform.Inventory.Infrastructure.Read;

internal sealed class InventoryReadService(ISqlConnectionFactory connectionFactory) : IInventoryReadService
{
    public async Task<StockResponse?> GetStockAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry,
        CancellationToken cancellationToken = default, Guid batchId = default)
    {
        originCountry =
            originCountry.Trim().ToUpperInvariant();

        const string sql = """
            SELECT
                b.Id,
                b.WarehouseId,
                b.ProductVarietyId,
                b.OnHand,
                b.Reserved,
                b.OnHand - b.Reserved AS Available,
                b.UpdatedAtUtc,
                p.Id AS ProductId,
                p.Name AS ProductName,
                pv.Name AS VarietyName,
                w.Name AS WarehouseName,
                w.Code AS WarehouseCode,
                b.OriginCountry,
                b.BatchId, b.BatchCode, b.PackWeightKg, b.FruitsPerPack,
                b.SellingPrice, b.SellingCurrency, b.SellingPriceUpdatedAtUtc
            FROM inventory.InventoryBalances b
            INNER JOIN warehousing.Warehouses w
                ON w.Id = b.WarehouseId
               AND w.TenantId = b.TenantId
            INNER JOIN catalog.ProductVarieties pv
                ON pv.Id = b.ProductVarietyId
            INNER JOIN catalog.Products p
                ON p.Id = pv.ProductId
               AND p.TenantId = b.TenantId
            WHERE b.TenantId = @TenantId
              AND b.WarehouseId = @WarehouseId
              AND b.ProductVarietyId = @ProductVarietyId
              AND b.OriginCountry = @OriginCountry AND b.BatchId = @BatchId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<StockResponse>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId, WarehouseId = warehouseId, ProductVarietyId = productVarietyId, OriginCountry = originCountry, BatchId = batchId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<StockSummaryResponse>> SearchStockAsync(
        Guid tenantId,
        Guid? warehouseId,
        Guid? productVarietyId,
        string? originCountry,
        CancellationToken cancellationToken = default)
    {
        originCountry =
            string.IsNullOrWhiteSpace(originCountry)
                ? null
                : originCountry.Trim().ToUpperInvariant();

        const string sql = """
            SELECT
                b.Id,
                b.WarehouseId,
                b.ProductVarietyId,
                b.OnHand,
                b.Reserved,
                b.OnHand - b.Reserved AS Available,
                b.UpdatedAtUtc,
                p.Id AS ProductId,
                p.Name AS ProductName,
                pv.Name AS VarietyName,
                w.Name AS WarehouseName,
                w.Code AS WarehouseCode,
                b.OriginCountry,
                b.BatchId, b.BatchCode, b.PackWeightKg, b.FruitsPerPack,
                b.SellingPrice, b.SellingCurrency, b.SellingPriceUpdatedAtUtc
            FROM inventory.InventoryBalances b
            INNER JOIN warehousing.Warehouses w
                ON w.Id = b.WarehouseId
               AND w.TenantId = b.TenantId
            INNER JOIN catalog.ProductVarieties pv
                ON pv.Id = b.ProductVarietyId
            INNER JOIN catalog.Products p
                ON p.Id = pv.ProductId
               AND p.TenantId = b.TenantId
            WHERE b.TenantId = @TenantId
              AND (@WarehouseId IS NULL OR b.WarehouseId = @WarehouseId)
              AND (@ProductVarietyId IS NULL OR b.ProductVarietyId = @ProductVarietyId)
              AND (@OriginCountry IS NULL OR b.OriginCountry = @OriginCountry)
            ORDER BY p.Name, pv.Name, w.Name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<StockSummaryResponse>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId, WarehouseId = warehouseId, ProductVarietyId = productVarietyId, OriginCountry = originCountry },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    public async Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry,
        CancellationToken cancellationToken = default, Guid batchId = default)
    {
        originCountry =
            originCountry.Trim().ToUpperInvariant();

        const string sql = """
            SELECT
                m.Id,
                CASE m.Type
                    WHEN 1 THEN 'Received'
                    WHEN 2 THEN 'Reserved'
                    WHEN 3 THEN 'ReservationReleased'
                    WHEN 4 THEN 'Dispatched'
                    WHEN 5 THEN 'Adjusted'
                    WHEN 6 THEN 'TransferOut'
                    WHEN 7 THEN 'TransferIn'
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
            INNER JOIN inventory.InventoryBalances b ON b.Id = m.InventoryBalanceId
            WHERE b.TenantId = @TenantId
              AND b.WarehouseId = @WarehouseId
              AND b.ProductVarietyId = @ProductVarietyId
              AND b.OriginCountry = @OriginCountry AND b.BatchId = @BatchId
            ORDER BY m.OccurredAtUtc DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<StockMovementResponse>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId, WarehouseId = warehouseId, ProductVarietyId = productVarietyId, OriginCountry = originCountry, BatchId = batchId },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }
}
