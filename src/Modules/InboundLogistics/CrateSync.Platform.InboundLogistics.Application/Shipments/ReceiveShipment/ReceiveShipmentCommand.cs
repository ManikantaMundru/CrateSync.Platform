using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;

public sealed record ReceiveShipmentCommand(
    Guid ReceiptId,
    Guid ShipmentId,
    Guid ShipmentLineId,
    Guid WarehouseId,
    int Quantity) : ICommand<Result>;
