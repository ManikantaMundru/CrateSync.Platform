namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentLineId(Guid Value)
{
    public static ShipmentLineId New() => new(Guid.NewGuid());

    public static ShipmentLineId From(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Shipment line ID cannot be empty.", nameof(value));

        return new ShipmentLineId(value);
    }

    public override string ToString() => Value.ToString();
}
