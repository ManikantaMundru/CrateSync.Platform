using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.AdjustStock;

public sealed record AdjustStockCommand(
    Guid WarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int QuantityChange,
    string Reason) : ICommand<Result>;
