namespace CrateSync.Platform.Inventory.Domain.ValueObjects;

public sealed record AdjustmentReason
{
    private AdjustmentReason() { Value = null!; }

    public AdjustmentReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Adjustment reason is required.", nameof(value));
        value = value.Trim();
        if (value.Length > 250) throw new ArgumentException("Adjustment reason cannot exceed 250 characters.", nameof(value));
        Value = value;
    }

    public string Value { get; }
}
