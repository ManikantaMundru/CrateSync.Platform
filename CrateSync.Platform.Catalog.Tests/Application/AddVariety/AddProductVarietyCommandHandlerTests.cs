using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.AddVariety;
using CrateSync.Platform.Catalog.Domain.Products;
using CrateSync.Platform.Catalog.Domain.Products.Events;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.AddVariety;

public sealed class AddProductVarietyCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly FakeTimeProvider _timeProvider;

    public AddProductVarietyCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<ICatalogUnitOfWork>();
        _tenantContext = Substitute.For<ITenantContext>();
        _timeProvider = new FakeTimeProvider(Now);
    }

    [Fact]
    public async Task Handle_Should_Add_Variety_When_Product_Exists()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create(tenantId, "Apple", Now);
        product.ClearDomainEvents();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new AddProductVarietyCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new AddProductVarietyCommand(product.Id.Value, "Royal Gala"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        product.Varieties.ShouldHaveSingleItem().Name.ShouldBe("Royal Gala");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();
        var productId = ProductId.New();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(productId, tenantId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var handler = new AddProductVarietyCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new AddProductVarietyCommand(productId.Value, "Royal Gala"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_Current_Tenant_When_Loading_Product()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create(tenantId, "Apple", Now);

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new AddProductVarietyCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new AddProductVarietyCommand(product.Id.Value, "Royal Gala"), CancellationToken.None);

        await _productRepository.Received(1).GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_TimeProvider_For_Domain_Event()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create(tenantId, "Apple", Now);
        product.ClearDomainEvents();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new AddProductVarietyCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new AddProductVarietyCommand(product.Id.Value, "Royal Gala"), CancellationToken.None);

        var domainEvent = product.DomainEvents.OfType<ProductVarietyAddedDomainEvent>().ShouldHaveSingleItem();

        domainEvent.OccurredOnUtc.ShouldBe(Now);
    }
}
