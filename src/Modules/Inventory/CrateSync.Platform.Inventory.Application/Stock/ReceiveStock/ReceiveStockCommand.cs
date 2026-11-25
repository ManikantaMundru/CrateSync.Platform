using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;

public sealed record ReceiveStockCommand(
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int Quantity,
    Guid ShipmentId,
    Guid? ReceiptId = null, Guid BatchId = default, string? BatchCode = null, decimal? PackWeightKg = null, int? FruitsPerPack = null, DateTimeOffset? OriginalReceivedAtUtc = null) : ICommand<Result>;
