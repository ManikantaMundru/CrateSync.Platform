namespace CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

public sealed record PackSize
{
    private PackSize() { }

    public PackSize(decimal weightKg)
    {
        if (weightKg <= 0) throw new ArgumentOutOfRangeException(nameof(weightKg), "Pack weight must be greater than zero.");
        WeightKg = weightKg;
    }

    private PackSize(int fruitCount, bool countBased)
    {
        if (fruitCount <= 0) throw new ArgumentOutOfRangeException(nameof(fruitCount));
        FruitCount = fruitCount;
    }

    public static PackSize ByCount(int fruitCount) => new(fruitCount, true);
    public decimal? WeightKg { get; private set; }
    public int? FruitCount { get; private set; }
    public override string ToString() => FruitCount is int count ? $"{count} fruits" : $"{WeightKg:0.###} kg";
}
