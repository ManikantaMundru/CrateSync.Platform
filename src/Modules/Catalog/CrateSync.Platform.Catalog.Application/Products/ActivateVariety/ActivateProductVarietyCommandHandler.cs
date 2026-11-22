using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.ActivateVariety;

internal sealed class ActivateProductVarietyCommandHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ITenantContext tenantContext)
    : ICommandHandler<
        ActivateProductVarietyCommand,
        Result>
{
    public async Task<Result> Handle(
        ActivateProductVarietyCommand request,
        CancellationToken cancellationToken)
    {
        var productId =
            ProductId.From(request.ProductId);

        var varietyId =
            ProductVarietyId.From(
                request.VarietyId);

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

        if (!product.Varieties.Any(
                x => x.Id == varietyId))
        {
            return Result.Failure(
                ProductErrors.VarietyNotFound(
                    productId,
                    varietyId));
        }

        product.ActivateVariety(varietyId);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}
