using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.Common;

public static class ShipmentErrors
{
    public static Error NotFound(Guid shipmentId) =>
        new(
            "InboundLogistics.Shipment.NotFound",
            $"Inbound shipment '{shipmentId}' was not found.",
            ErrorType.NotFound);

    public static Error TrackingNumberAlreadyExists(string trackingNumber) =>
        new(
            "InboundLogistics.Shipment.TrackingNumberAlreadyExists",
            $"Tracking number '{trackingNumber}' already exists.",
            ErrorType.Conflict);
}
