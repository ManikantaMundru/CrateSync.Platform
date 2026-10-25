using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Inventory.Application.Stock.SearchStock;

public sealed record SearchStockQuery(
    Guid? WarehouseId,
    Guid? ProductVarietyId)
    : IQuery<IReadOnlyCollection<StockSummaryResponse>>;
