namespace CrateSync.Platform.Inventory.Domain.ValueObjects;

public sealed record AdjustmentReason
{
    private AdjustmentReason() { }

    public AdjustmentReason(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim();

        if (normalized.Length > 250)
            throw new ArgumentException(
                "Adjustment reason cannot exceed 250 characters.",
                nameof(value));

        Value = normalized;
    }

    public string Value { get; private set; } = string.Empty;

    public override string ToString() => Value;
}
