using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.ReserveStock;

public sealed record ReserveStockCommand(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId) : ICommand<Result>;
