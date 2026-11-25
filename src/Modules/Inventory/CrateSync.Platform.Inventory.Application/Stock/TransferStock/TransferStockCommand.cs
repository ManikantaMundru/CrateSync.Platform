using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

public sealed record TransferStockCommand(
    Guid TransferId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int Quantity,
    string? TransferReference,
    string? Remarks, Guid BatchId = default, string TransportCompany = "", string DriverName = "", string DriverPhone = "", string? VehicleNumber = null) : ICommand<Result>;
