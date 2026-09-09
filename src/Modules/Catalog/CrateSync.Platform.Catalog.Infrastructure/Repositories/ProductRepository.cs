using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Domain.Products;
using CrateSync.Platform.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Catalog.Infrastructure.Repositories;

internal sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(
        ProductId productId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .Include(x => x.Varieties)
            .SingleOrDefaultAsync(
                x =>
                    x.Id == productId &&
                    x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        Guid tenantId,
        string name,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Products
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.Name == name,
                cancellationToken);
    }

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }
}
