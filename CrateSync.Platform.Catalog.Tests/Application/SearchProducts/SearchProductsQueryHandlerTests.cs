using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.SearchProducts;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.SearchProducts;

public sealed class SearchProductsQueryHandlerTests
{
    private readonly IProductReadService _productReadService;
    private readonly ITenantContext _tenantContext;

    public SearchProductsQueryHandlerTests()
    {
        _productReadService = Substitute.For<IProductReadService>();
        _tenantContext = Substitute.For<ITenantContext>();
    }

    [Fact]
    public async Task Handle_Should_Return_Products()
    {
        var tenantId = Guid.NewGuid();

        IReadOnlyCollection<ProductSearchResponse> products =
        [
            new ProductSearchResponse(Guid.NewGuid(), "Apple", true, 2),
            new ProductSearchResponse(Guid.NewGuid(), "Pear", true, 1)
        ];

        _tenantContext.TenantId.Returns(tenantId);
        _productReadService.SearchAsync(tenantId, null, null, Arg.Any<CancellationToken>()).Returns(products);

        var handler = new SearchProductsQueryHandler(_productReadService, _tenantContext);

        var result = await handler.Handle(new SearchProductsQuery(null, null), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.ShouldBe(products);
    }

    [Fact]
    public async Task Handle_Should_Pass_SearchTerm_To_ReadService()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _productReadService
            .SearchAsync(tenantId, "Apple", null, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ProductSearchResponse>());

        var handler = new SearchProductsQueryHandler(_productReadService, _tenantContext);

        await handler.Handle(new SearchProductsQuery("Apple", null), CancellationToken.None);

        await _productReadService.Received(1).SearchAsync(tenantId, "Apple", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Pass_IsActive_Filter_To_ReadService()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _productReadService
            .SearchAsync(tenantId, null, true, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ProductSearchResponse>());

        var handler = new SearchProductsQueryHandler(_productReadService, _tenantContext);

        await handler.Handle(new SearchProductsQuery(null, true), CancellationToken.None);

        await _productReadService.Received(1).SearchAsync(tenantId, null, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Query_Using_Current_Tenant()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _productReadService
            .SearchAsync(tenantId, null, null, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<ProductSearchResponse>());

        var handler = new SearchProductsQueryHandler(_productReadService, _tenantContext);

        await handler.Handle(new SearchProductsQuery(null, null), CancellationToken.None);

        await _productReadService.Received(1).SearchAsync(tenantId, null, null, Arg.Any<CancellationToken>());
    }
}
