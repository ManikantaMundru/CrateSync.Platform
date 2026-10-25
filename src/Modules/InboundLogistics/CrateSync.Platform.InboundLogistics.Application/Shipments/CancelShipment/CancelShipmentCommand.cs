using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CancelShipment;

public sealed record CancelShipmentCommand(
    Guid ShipmentId) : ICommand<Result>;
