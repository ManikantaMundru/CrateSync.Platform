using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ConfirmWarehouseDelivery;

public sealed record ConfirmWarehouseDeliveryCommand(
    Guid ShipmentId,
    Guid DeliveryId) : ICommand<Result>;
