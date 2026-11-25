namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum ShipmentDeliveryStatus
{
    Planned = 1,
    InTransit = 2,
    Received = 3,
    Cancelled = 4,
    DeliveredToWarehouse = 5
}
