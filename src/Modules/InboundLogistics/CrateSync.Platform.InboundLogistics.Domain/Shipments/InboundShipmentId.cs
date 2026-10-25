namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct InboundShipmentId(Guid Value)
{
    public static InboundShipmentId New() => new(Guid.NewGuid());

    public static InboundShipmentId From(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Inbound shipment ID cannot be empty.", nameof(value));

        return new InboundShipmentId(value);
    }

    public override string ToString() => Value.ToString();
}
