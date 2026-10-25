using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;

public sealed record ReceiveShipmentCommand(
    Guid ShipmentId,
    Guid ShipmentLineId,
    int Quantity) : ICommand<Result>;
