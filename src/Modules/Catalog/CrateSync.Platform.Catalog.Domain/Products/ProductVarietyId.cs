namespace CrateSync.Platform.Catalog.Domain.Products;

public readonly record struct ProductVarietyId(Guid Value)
{
    public static ProductVarietyId New()
    {
        return new ProductVarietyId(Guid.NewGuid());
    }

    public static ProductVarietyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Product variety ID cannot be empty.",
                nameof(value));
        }

        return new ProductVarietyId(value);
    }

    public override string ToString() => Value.ToString();
}
