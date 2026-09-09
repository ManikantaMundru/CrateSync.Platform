namespace CrateSync.Platform.Catalog.Domain.Products;

public readonly record struct ProductId(Guid Value)
{
    public static ProductId New()
    {
        return new ProductId(Guid.NewGuid());
    }

    public static ProductId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "Product ID cannot be empty.",
                nameof(value));
        }

        return new ProductId(value);
    }

    public override string ToString() => Value.ToString();
}
