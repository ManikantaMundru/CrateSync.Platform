using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.ReleaseStockReservation;

public sealed record ReleaseStockReservationCommand(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId) : ICommand<Result>;
