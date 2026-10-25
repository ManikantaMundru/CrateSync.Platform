namespace CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

public sealed record PackSize
{
    private PackSize() { }

    public PackSize(decimal weightKg)
    {
        if (weightKg <= 0) throw new ArgumentOutOfRangeException(nameof(weightKg), "Pack size must be greater than zero.");

        WeightKg = weightKg;
    }

    public decimal WeightKg { get; private set; }

    public override string ToString() => $"{WeightKg:0.##} kg";
}
