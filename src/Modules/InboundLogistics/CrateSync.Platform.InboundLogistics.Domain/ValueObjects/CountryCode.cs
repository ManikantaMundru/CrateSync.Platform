namespace CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

public sealed record CountryCode
{
    private CountryCode() { }

    public CountryCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2) throw new ArgumentException("Country code must be a 2-character ISO code.", nameof(value));

        Value = normalized;
    }

    public string Value { get; private set; } = string.Empty;

    public override string ToString() => Value;
}
