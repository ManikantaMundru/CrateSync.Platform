namespace CrateSync.Platform.Catalog.Domain.Products;

public readonly record struct ProductVarietyId(Guid Value)
{
    public static ProductVarietyId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
