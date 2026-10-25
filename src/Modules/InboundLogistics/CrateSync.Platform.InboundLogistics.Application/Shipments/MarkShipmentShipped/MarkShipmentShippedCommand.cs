using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.MarkShipmentShipped;

public sealed record MarkShipmentShippedCommand(
    Guid ShipmentId) : ICommand<Result>;
