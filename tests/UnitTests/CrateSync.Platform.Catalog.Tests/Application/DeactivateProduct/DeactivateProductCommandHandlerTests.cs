using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.DeactivateProduct;
using CrateSync.Platform.Catalog.Domain.Products;
using CrateSync.Platform.Catalog.Domain.Products.Events;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.DeactivateProduct;

public sealed class DeactivateProductCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly FakeTimeProvider _timeProvider;

    public DeactivateProductCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<ICatalogUnitOfWork>();
        _tenantContext = Substitute.For<ITenantContext>();
        _timeProvider = new FakeTimeProvider(Now);
    }

    [Fact]
    public async Task Handle_Should_Deactivate_Product_When_Product_Exists()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create(tenantId, "Apple", Now);

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new DeactivateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new DeactivateProductCommand(product.Id.Value), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        product.IsActive.ShouldBeFalse();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();
        var productId = ProductId.New();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(productId, tenantId, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var handler = new DeactivateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new DeactivateProductCommand(productId.Value), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Raise_ProductDeactivatedDomainEvent()
    {
        var tenantId = Guid.NewGuid();
        var product = Product.Create(tenantId, "Apple", Now);
        product.ClearDomainEvents();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.GetByIdAsync(product.Id, tenantId, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new DeactivateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new DeactivateProductCommand(product.Id.Value), CancellationToken.None);

        var domainEvent = product.DomainEvents.OfType<ProductDeactivatedDomainEvent>().ShouldHaveSingleItem();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.OccurredOnUtc.ShouldBe(Now);
    }
}
