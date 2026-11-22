namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentClearanceEventId(Guid Value)
{
    public static ShipmentClearanceEventId New() => new(Guid.NewGuid());

    public static ShipmentClearanceEventId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Clearance event ID cannot be empty.", nameof(value));

        return new ShipmentClearanceEventId(value);
    }
}
