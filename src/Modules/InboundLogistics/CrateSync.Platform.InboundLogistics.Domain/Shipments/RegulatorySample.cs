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
    public DateTimeOffset? ReversedAtUtc { get; private set; }
    public string? ReversalReason { get; private set; }
    internal void Reverse(string reason, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
            throw new InvalidOperationException("A reversal reason of up to 500 characters is required.");
        if (ReversedAtUtc.HasValue)
        {
            if (ReversalReason != reason.Trim()) throw new InvalidOperationException("Sample already reversed.");
            return;
        }
        ReversedAtUtc = now;
        ReversalReason = reason.Trim();
    }
}
