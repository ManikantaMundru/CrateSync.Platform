using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        ProductId productId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default);

    void Add(Product product);
}
