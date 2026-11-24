using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.DispatchWarehouseDelivery;

public sealed record DispatchWarehouseDeliveryCommand(
    Guid ShipmentId,
    Guid DeliveryId) : ICommand<Result>;
