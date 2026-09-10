using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.CreateProduct;
using CrateSync.Platform.Catalog.Domain.Products;
using CrateSync.Platform.Catalog.Domain.Products.Events;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Application.CreateProduct;

public sealed class CreateProductCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly FakeTimeProvider _timeProvider;

    public CreateProductCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<ICatalogUnitOfWork>();
        _tenantContext = Substitute.For<ITenantContext>();
        _timeProvider = new FakeTimeProvider(Now);
    }

    [Fact]
    public async Task Handle_Should_Create_Product_When_Name_Is_Unique()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.ExistsByNameAsync(tenantId, "Apple", Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new CreateProductCommand("Apple"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        _productRepository.Received(1).Add(Arg.Is<Product>(x => x.Name == "Apple" && x.TenantId == tenantId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Use_Current_Tenant()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.ExistsByNameAsync(tenantId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new CreateProductCommand("Apple"), CancellationToken.None);

        _productRepository.Received(1).Add(Arg.Is<Product>(x => x.TenantId == tenantId));
    }

    [Fact]
    public async Task Handle_Should_Use_TimeProvider_For_Domain_Event()
    {
        var tenantId = Guid.NewGuid();
        Product? capturedProduct = null;

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.ExistsByNameAsync(tenantId, "Apple", Arg.Any<CancellationToken>()).Returns(false);

        _productRepository.When(x => x.Add(Arg.Any<Product>())).Do(x => capturedProduct = x.Arg<Product>());

        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new CreateProductCommand("Apple"), CancellationToken.None);

        capturedProduct.ShouldNotBeNull();

        var domainEvent = capturedProduct.DomainEvents.OfType<ProductCreatedDomainEvent>().ShouldHaveSingleItem();
        domainEvent.OccurredOnUtc.ShouldBe(Now);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Product_Name_Already_Exists()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.ExistsByNameAsync(tenantId, "Apple", Arg.Any<CancellationToken>()).Returns(true);

        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        var result = await handler.Handle(new CreateProductCommand("Apple"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();

        _productRepository.DidNotReceive().Add(Arg.Any<Product>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Check_Duplicate_Name_Within_Current_Tenant()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);
        _productRepository.ExistsByNameAsync(tenantId, "Apple", Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateProductCommandHandler(_productRepository, _unitOfWork, _tenantContext, _timeProvider);

        await handler.Handle(new CreateProductCommand("Apple"), CancellationToken.None);

        await _productRepository.Received(1).ExistsByNameAsync(tenantId, "Apple", Arg.Any<CancellationToken>());
    }
}
