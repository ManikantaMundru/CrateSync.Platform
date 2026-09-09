using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.GetProduct;

internal sealed class GetProductQueryHandler(
    IProductReadService productReadService,
    ITenantContext tenantContext) : IQueryHandler<GetProductQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        var response =
            await productReadService.GetByIdAsync(
                tenantContext.TenantId,
                request.ProductId,
                cancellationToken);

        if (response is null)
        {
            return Result<ProductResponse>.Failure(
                ProductErrors.NotFound(
                    ProductId.From(request.ProductId)));
        }

        return Result<ProductResponse>.Success(
            response);
    }
}
