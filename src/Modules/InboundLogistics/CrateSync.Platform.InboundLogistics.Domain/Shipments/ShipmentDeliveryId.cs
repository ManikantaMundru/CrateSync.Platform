namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentDeliveryId(Guid Value)
{
    public static ShipmentDeliveryId New() => new(Guid.NewGuid());

    public static ShipmentDeliveryId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Shipment delivery ID cannot be empty.", nameof(value));

        return new ShipmentDeliveryId(value);
    }
}
