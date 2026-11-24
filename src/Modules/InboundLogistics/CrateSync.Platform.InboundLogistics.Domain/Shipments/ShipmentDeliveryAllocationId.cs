namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentDeliveryAllocationId(Guid Value)
{
    public static ShipmentDeliveryAllocationId New() => new(Guid.NewGuid());

    public static ShipmentDeliveryAllocationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Shipment delivery allocation ID cannot be empty.", nameof(value));

        return new ShipmentDeliveryAllocationId(value);
    }
}
