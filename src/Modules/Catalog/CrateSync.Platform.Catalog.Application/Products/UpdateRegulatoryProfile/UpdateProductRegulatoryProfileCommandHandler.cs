using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.UpdateRegulatoryProfile;

internal sealed class UpdateProductRegulatoryProfileCommandHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ITenantContext tenantContext)
    : ICommandHandler<UpdateProductRegulatoryProfileCommand, Result>
{
    public async Task<Result> Handle(
        UpdateProductRegulatoryProfileCommand request,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(request.ProductId);

        var product = await productRepository.GetByIdAsync(
            productId,
            tenantContext.TenantId,
            cancellationToken);

        if (product is null)
            return Result.Failure(
                ProductErrors.NotFound(productId));

        product.UpdateRegulatoryProfile(
            request.CustomsTariffCode,
            request.ScientificName,
            request.RegulatoryCategory,
            request.DefaultUnitOfMeasure);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
