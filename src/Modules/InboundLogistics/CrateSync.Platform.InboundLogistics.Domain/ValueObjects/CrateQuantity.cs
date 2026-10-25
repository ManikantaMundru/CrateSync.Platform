namespace CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

public readonly record struct CrateQuantity
{
    public CrateQuantity(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Crate quantity cannot be negative.");

        Value = value;
    }

    public int Value { get; }

    public static CrateQuantity Zero => new(0);

    public static CrateQuantity operator +(CrateQuantity left, CrateQuantity right) => new(left.Value + right.Value);

    public static CrateQuantity operator -(CrateQuantity left, CrateQuantity right)
    {
        if (right.Value > left.Value) throw new InvalidOperationException("Crate quantity cannot become negative.");

        return new CrateQuantity(left.Value - right.Value);
    }

    public override string ToString() => Value.ToString();
}
