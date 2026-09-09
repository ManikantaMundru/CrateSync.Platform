using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Catalog.Domain.Products;

public sealed class ProductVariety : Entity<ProductVarietyId>
{
    private ProductVariety()
    {
    }

    internal ProductVariety(ProductVarietyId id, string name) : base(id)
    {
        Name = ValidateName(name);
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    internal void Rename(string name)
    {
        Name = ValidateName(name);
    }

    internal void Activate()
    {
        IsActive = true;
    }

    internal void Deactivate()
    {
        IsActive = false;
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();

        if (trimmedName.Length > 250)
        {
            throw new ArgumentException(
                "Product variety name cannot exceed 250 characters.",
                nameof(name));
        }

        return trimmedName;
    }
}
