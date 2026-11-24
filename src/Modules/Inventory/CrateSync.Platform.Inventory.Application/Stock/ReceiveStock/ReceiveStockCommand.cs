using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;

public sealed record ReceiveStockCommand(
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    string CountryOfOrigin,
    int Quantity,
    Guid ShipmentId,
    Guid? ReceiptId = null) : ICommand<Result>;
