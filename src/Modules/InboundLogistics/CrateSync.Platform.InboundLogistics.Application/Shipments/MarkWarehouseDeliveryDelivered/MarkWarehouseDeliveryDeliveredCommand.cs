using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.MarkWarehouseDeliveryDelivered;

public sealed record MarkWarehouseDeliveryDeliveredCommand(
    Guid ShipmentId,
    Guid DeliveryId) : ICommand<Result>;
