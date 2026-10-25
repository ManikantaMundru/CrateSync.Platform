namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed record TrackingNumber
{
    private TrackingNumber() { }

    public TrackingNumber(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();

        if (normalized.Length > 100) throw new ArgumentException("Tracking number cannot exceed 100 characters.", nameof(value));

        Value = normalized;
    }

    public string Value { get; private set; } = string.Empty;

    public override string ToString() => Value;
}
