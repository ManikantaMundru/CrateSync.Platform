namespace CrateSync.Platform.Sales.Domain.ValueObjects;

public readonly record struct PackSize
{
    public PackSize(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Pack size must be greater than zero.");
        Value = value;
    }

    public decimal Value { get; }
}
