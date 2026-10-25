namespace CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

public sealed record Currency
{
    private Currency() { }

    public Currency(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length != 3) throw new ArgumentException("Currency code must be a 3-character ISO code.", nameof(code));

        Code = normalized;
    }

    public string Code { get; private set; } = string.Empty;

    public override string ToString() => Code;
}
