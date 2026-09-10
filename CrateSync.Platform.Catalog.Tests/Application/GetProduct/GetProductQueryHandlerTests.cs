using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.GetProduct;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.GetProduct;

public sealed class GetProductQueryHandlerTests
{
    private readonly IProductReadService _productReadService;
    private readonly ITenantContext _tenantContext;

    public GetProductQueryHandlerTests()
    {
        _productReadService = Substitute.For<IProductReadService>();
        _tenantContext = Substitute.For<ITenantContext>();
    }

    [Fact]
    public async Task Handle_Should_Return_Product_When_Product_Exists()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var product = new ProductResponse(
            productId,
            "Apple",
            true,
            [
                new ProductVarietyResponse(Guid.NewGuid(), "Royal Gala", true)
            ]);

        _tenantContext.TenantId.Returns(tenantId);
        _productReadService.GetByIdAsync(tenantId, productId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new GetProductQueryHandler(_productReadService, _tenantContext);

        var result = await handler.Handle(new GetProductQuery(productId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(product);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productReadService.GetByIdAsync(tenantId, productId, Arg.Any<CancellationToken>()).Returns((ProductResponse?)null);

        var handler = new GetProductQueryHandler(_productReadService, _tenantContext);

        var result = await handler.Handle(new GetProductQuery(productId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Should_Query_Product_For_Current_Tenant()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productReadService.GetByIdAsync(tenantId, productId, Arg.Any<CancellationToken>()).Returns((ProductResponse?)null);

        var handler = new GetProductQueryHandler(_productReadService, _tenantContext);

        await handler.Handle(new GetProductQuery(productId), CancellationToken.None);

        await _productReadService.Received(1).GetByIdAsync(tenantId, productId, Arg.Any<CancellationToken>());
    }
}
