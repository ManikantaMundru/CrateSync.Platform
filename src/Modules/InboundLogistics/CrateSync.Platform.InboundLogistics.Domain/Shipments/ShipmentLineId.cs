namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public readonly record struct ShipmentLineId(Guid Value)
{
    public static ShipmentLineId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
