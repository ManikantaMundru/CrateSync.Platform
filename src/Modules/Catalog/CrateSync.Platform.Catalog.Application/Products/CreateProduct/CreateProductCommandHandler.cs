using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        var exists =
            await productRepository.ExistsByNameAsync(
                tenantId,
                request.Name,
                cancellationToken);

        if (exists)
        {
            return Result<Guid>.Failure(
                ProductErrors.DuplicateName(request.Name));
        }

        var occurredOnUtc =
            timeProvider.GetUtcNow();

        var product =
            Product.Create(
                tenantId,
                request.Name,
                occurredOnUtc);

        productRepository.Add(product);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            product.Id.Value);
    }
}
