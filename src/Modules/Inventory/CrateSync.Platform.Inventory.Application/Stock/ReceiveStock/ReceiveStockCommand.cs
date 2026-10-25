using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;

public sealed record ReceiveStockCommand(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid ShipmentId) : ICommand<Result>;
