using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Catalog.Domain.Products.Events;

namespace CrateSync.Platform.Catalog.Domain.Products;

public sealed class Product : AggregateRoot<ProductId>
{
    private readonly List<ProductVariety> _varieties = [];

    private Product()
    {
    }

    private Product(ProductId id, Guid tenantId, string name) : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        TenantId = tenantId;
        Name = ValidateName(name);
        IsActive = true;
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<ProductVariety> Varieties =>
        _varieties.AsReadOnly();

    public static Product Create(
        Guid tenantId,
        string name,
        DateTimeOffset occurredAtUtc)
    {
        var product = new Product(
            ProductId.New(),
            tenantId,
            name);

        product.RaiseDomainEvent(
            new ProductCreatedDomainEvent(
                product.Id,
                product.TenantId,
                product.Name,
                occurredAtUtc));

        return product;
    }

    public void Rename(string name)
    {
        Name = ValidateName(name);
    }

    public ProductVarietyId AddVariety(string name, DateTimeOffset occurredAtUtc)
    {
        EnsureActive();

        var normalizedName = ValidateName(name);

        var alreadyExists = _varieties.Any(
            x => string.Equals(
                x.Name,
                normalizedName,
                StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                $"A variety named '{normalizedName}' already exists.");
        }

        var variety = new ProductVariety(
            ProductVarietyId.New(),
            normalizedName);

        _varieties.Add(variety);

        RaiseDomainEvent(
            new ProductVarietyAddedDomainEvent(
                Id,
                variety.Id,
                variety.Name,
                occurredAtUtc));

        return variety.Id;
    }

    public void RenameVariety(ProductVarietyId varietyId, string name)
    {
        EnsureActive();

        var variety = GetVariety(varietyId);

        var normalizedName = ValidateName(name);

        var duplicateExists = _varieties.Any(
            x =>
                x.Id != varietyId &&
                string.Equals(
                    x.Name,
                    normalizedName,
                    StringComparison.OrdinalIgnoreCase));

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"A variety named '{normalizedName}' already exists.");
        }

        variety.Rename(normalizedName);
    }

    public void DeactivateVariety(
        ProductVarietyId varietyId)
    {
        var variety = GetVariety(varietyId);

        variety.Deactivate();
    }

    public void ActivateVariety(
        ProductVarietyId varietyId)
    {
        EnsureActive();

        var variety = GetVariety(varietyId);

        variety.Activate();
    }

    public void Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        RaiseDomainEvent(
            new ProductDeactivatedDomainEvent(
                Id,
                TenantId,
                occurredAtUtc));
    }

    public void Activate()
    {
        IsActive = true;
    }

    private ProductVariety GetVariety(ProductVarietyId varietyId)
    {
        var variety = _varieties.SingleOrDefault(x => x.Id == varietyId);

        return variety is null
            ? throw new InvalidOperationException(
                $"Product variety '{varietyId}' was not found.")
            : variety;
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException(
                "An inactive product cannot be modified.");
        }
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();

        if (trimmedName.Length > 150)
        {
            throw new ArgumentException(
                "Product name cannot exceed 150 characters.",
                nameof(name));
        }

        return trimmedName;
    }
}
