namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class RegulatorySample
{
    private RegulatorySample() { }
    internal RegulatorySample(Guid id, Guid shipmentLineId, ClearanceAuthority authority, int quantity,
        string? referenceNumber, string remarks, DateTimeOffset recordedAtUtc)
    {
        Id = id;
        ShipmentLineId = shipmentLineId;
        Authority = authority;
        Quantity = quantity;
        ReferenceNumber = referenceNumber?.Trim();
        Remarks = remarks.Trim();
        RecordedAtUtc = recordedAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid ShipmentLineId { get; private set; }
    public ClearanceAuthority Authority { get; private set; }
    public int Quantity { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string Remarks { get; private set; } = null!;
    public DateTimeOffset RecordedAtUtc { get; private set; }
}
