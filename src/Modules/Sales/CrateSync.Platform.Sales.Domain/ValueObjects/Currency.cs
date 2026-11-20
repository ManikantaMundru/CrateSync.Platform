namespace CrateSync.Platform.Sales.Domain.ValueObjects;

public sealed record Currency
{
    private Currency() { Code = null!; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Currency is required.", nameof(code));
        code = code.Trim().ToUpperInvariant();
        if (code.Length != 3) throw new ArgumentException("Currency must be a 3-character ISO code.", nameof(code));
        Code = code;
    }

    public string Code { get; }
    public override string ToString() => Code;
}
