using CrateSync.Platform.Catalog.Application.Products.GetProduct;
using CrateSync.Platform.Catalog.Application.Products.SearchProducts;

namespace CrateSync.Platform.Catalog.Application.Abstractions
{
    public interface IProductReadService
    {
        Task<ProductResponse?> GetByIdAsync(
            Guid tenantId,
            Guid productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ProductSearchResponse>> SearchAsync(
            Guid tenantId,
            string? searchTerm,
            bool? isActive,
            CancellationToken cancellationToken = default);
    }
}
