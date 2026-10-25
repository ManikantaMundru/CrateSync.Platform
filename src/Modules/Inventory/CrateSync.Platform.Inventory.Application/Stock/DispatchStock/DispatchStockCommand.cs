using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.DispatchStock;

public sealed record DispatchStockCommand(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId) : ICommand<Result>;
