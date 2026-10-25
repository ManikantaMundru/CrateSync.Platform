using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Inventory.Application.Stock.GetStock;

public sealed record GetStockQuery(
    Guid WarehouseId,
    Guid ProductVarietyId)
    : IQuery<Result<StockResponse>>;
