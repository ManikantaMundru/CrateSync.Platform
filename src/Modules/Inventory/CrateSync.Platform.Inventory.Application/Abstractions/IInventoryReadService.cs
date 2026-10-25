using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;
using CrateSync.Platform.Inventory.Application.Stock.SearchStock;

namespace CrateSync.Platform.Inventory.Application.Abstractions;

public interface IInventoryReadService
{
    Task<StockResponse?> GetStockAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockSummaryResponse>> SearchStockAsync(
        Guid tenantId,
        Guid? warehouseId,
        Guid? productVarietyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StockMovementResponse>> GetMovementsAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default);
}
