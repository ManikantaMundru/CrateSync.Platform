namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public enum ShipmentStatus
{
    Draft = 1,
    Shipped = 2,
    Arrived = 3,
    Receiving = 4,
    PartiallyReceived = 5,
    Received = 6,
    Cancelled = 7
}
