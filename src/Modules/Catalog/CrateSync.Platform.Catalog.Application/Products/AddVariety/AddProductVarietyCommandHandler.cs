using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.common;
using CrateSync.Platform.Catalog.Domain.Products;

namespace CrateSync.Platform.Catalog.Application.Products.AddVariety;

internal sealed class AddProductVarietyCommandHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<AddProductVarietyCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Guid>> Handle(
        AddProductVarietyCommand request,
        CancellationToken cancellationToken)
    {
        var productId =
            ProductId.From(request.ProductId);

        var product =
            await _productRepository.GetByIdAsync(
                productId,
                _tenantContext.TenantId,
                cancellationToken);

        if (product is null)
        {
            return Result<Guid>.Failure(
                ProductErrors.NotFound(productId));
        }

        var occurredOnUtc =
            _timeProvider.GetUtcNow();

        var varietyId =
            product.AddVariety(
                request.Name,
                occurredOnUtc);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            varietyId.Value);
    }
}
