using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.DeactivateProduct;

internal sealed class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<DeactivateProductCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productId =
            ProductId.From(request.ProductId);

        var product =
            await productRepository.GetByIdAsync(
                productId,
                tenantContext.TenantId,
                cancellationToken);

        if (product is null)
        {
            return Result.Failure(
                ProductErrors.NotFound(productId));
        }

        product.Deactivate(
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
