using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentArrived;

public sealed record MarkShipmentArrivedCommand(
    Guid ShipmentId) : ICommand<Result>;
