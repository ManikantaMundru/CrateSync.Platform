using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;

public sealed record GetStockMovementsQuery(
    Guid WarehouseId,
    Guid ProductVarietyId)
    : IQuery<IReadOnlyCollection<StockMovementResponse>>;
