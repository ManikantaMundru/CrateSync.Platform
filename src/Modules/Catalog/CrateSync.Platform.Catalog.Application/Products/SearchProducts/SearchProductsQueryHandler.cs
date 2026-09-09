using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Catalog.Application.Abstractions;

namespace CrateSync.Platform.Catalog.Application.Products.SearchProducts;

internal sealed class SearchProductsQueryHandler(
    IProductReadService productReadService,
    ITenantContext tenantContext) : IQueryHandler<SearchProductsQuery, IReadOnlyCollection<ProductSearchResponse>>
{
    public Task<IReadOnlyCollection<ProductSearchResponse>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        return productReadService.SearchAsync(
            tenantContext.TenantId,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);
    }
}
