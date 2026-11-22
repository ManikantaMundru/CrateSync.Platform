namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentReceiptId(Guid Value)
{
    public static ShipmentReceiptId New() => new(Guid.NewGuid());

    public static ShipmentReceiptId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Shipment receipt ID cannot be empty.", nameof(value));

        return new ShipmentReceiptId(value);
    }
}
