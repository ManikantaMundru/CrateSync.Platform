namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed record BatchCode
{
    private BatchCode() { }

    public BatchCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length > 100) throw new ArgumentException("Batch code cannot exceed 100 characters.", nameof(value));

        Value = normalized;
    }

    public string Value { get; private set; } = string.Empty;

    public override string ToString() => Value;
}
