namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentDocumentId(Guid Value)
{
    public static ShipmentDocumentId New() => new(Guid.NewGuid());

    public static ShipmentDocumentId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Shipment document ID cannot be empty.", nameof(value));

        return new ShipmentDocumentId(value);
    }
}
