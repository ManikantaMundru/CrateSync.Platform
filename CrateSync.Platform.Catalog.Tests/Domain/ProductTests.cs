using CrateSync.Platform.Catalog.Domain.Products;
using CrateSync.Platform.Catalog.Domain.Products.Events;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Catalog.Tests.Domain;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Should_Create_Active_Product()
    {
        var tenantId = Guid.NewGuid();

        var product = Product.Create(tenantId, "Apple", Now);

        product.Id.Value.ShouldNotBe(Guid.Empty);
        product.TenantId.ShouldBe(tenantId);
        product.Name.ShouldBe("Apple");
        product.IsActive.ShouldBeTrue();
        product.Varieties.ShouldBeEmpty();
    }

    [Fact]
    public void Create_Should_Trim_Product_Name()
    {
        var product = Product.Create(Guid.NewGuid(), "  Apple  ", Now);

        product.Name.ShouldBe("Apple");
    }

    [Fact]
    public void Create_Should_Throw_When_TenantId_Is_Empty()
    {
        Should.Throw<ArgumentException>(() => Product.Create(Guid.Empty, "Apple", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_Should_Throw_When_Name_Is_Empty(string name)
    {
        Should.Throw<ArgumentException>(() => Product.Create(Guid.NewGuid(), name, Now));
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Exceeds_Maximum_Length()
    {
        var name = new string('A', 151);

        Should.Throw<ArgumentException>(() => Product.Create(Guid.NewGuid(), name, Now));
    }

    [Fact]
    public void Create_Should_Raise_ProductCreatedDomainEvent()
    {
        var tenantId = Guid.NewGuid();

        var product = Product.Create(tenantId, "Apple", Now);

        var domainEvent = product.DomainEvents.OfType<ProductCreatedDomainEvent>().ShouldHaveSingleItem();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.TenantId.ShouldBe(tenantId);
        domainEvent.Name.ShouldBe("Apple");
        domainEvent.OccurredOnUtc.ShouldBe(Now);
        domainEvent.EventId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void AddVariety_Should_Add_Variety()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);

        var varietyId = product.AddVariety("Royal Gala", Now);

        var variety = product.Varieties.ShouldHaveSingleItem();

        variety.Id.ShouldBe(varietyId);
        variety.Name.ShouldBe("Royal Gala");
        variety.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void AddVariety_Should_Trim_Variety_Name()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);

        product.AddVariety("  Royal Gala  ", Now);

        product.Varieties.Single().Name.ShouldBe("Royal Gala");
    }

    [Fact]
    public void AddVariety_Should_Raise_ProductVarietyAddedDomainEvent()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.ClearDomainEvents();

        var varietyId = product.AddVariety("Royal Gala", Now);

        var domainEvent = product.DomainEvents.OfType<ProductVarietyAddedDomainEvent>().ShouldHaveSingleItem();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.ProductVarietyId.ShouldBe(varietyId);
        domainEvent.Name.ShouldBe("Royal Gala");
        domainEvent.OccurredOnUtc.ShouldBe(Now);
    }

    [Fact]
    public void AddVariety_Should_Reject_Duplicate_Name()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.AddVariety("Royal Gala", Now);

        Should.Throw<InvalidOperationException>(() => product.AddVariety("Royal Gala", Now));
    }

    [Fact]
    public void AddVariety_Should_Reject_Duplicate_Name_Ignoring_Case()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.AddVariety("Royal Gala", Now);

        Should.Throw<InvalidOperationException>(() => product.AddVariety("royal gala", Now));
    }

    [Fact]
    public void AddVariety_Should_Reject_Duplicate_Name_With_Whitespace()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.AddVariety("Royal Gala", Now);

        Should.Throw<InvalidOperationException>(() => product.AddVariety("  Royal Gala  ", Now));
    }

    [Fact]
    public void AddVariety_Should_Throw_When_Product_Is_Inactive()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.Deactivate(Now);

        Should.Throw<InvalidOperationException>(() => product.AddVariety("Royal Gala", Now));
    }

    [Fact]
    public void Deactivate_Should_Mark_Product_Inactive()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);

        product.Deactivate(Now);

        product.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Deactivate_Should_Raise_ProductDeactivatedDomainEvent()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.ClearDomainEvents();

        product.Deactivate(Now);

        var domainEvent = product.DomainEvents.OfType<ProductDeactivatedDomainEvent>().ShouldHaveSingleItem();

        domainEvent.ProductId.ShouldBe(product.Id);
        domainEvent.TenantId.ShouldBe(product.TenantId);
        domainEvent.OccurredOnUtc.ShouldBe(Now);
    }

    [Fact]
    public void Deactivate_Should_Not_Raise_Duplicate_Event_When_Already_Inactive()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);

        product.Deactivate(Now);
        product.ClearDomainEvents();

        product.Deactivate(Now);

        product.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Activate_Should_Mark_Product_Active()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);
        product.Deactivate(Now);

        product.Activate();

        product.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_DomainEvents()
    {
        var product = Product.Create(Guid.NewGuid(), "Apple", Now);

        product.DomainEvents.ShouldNotBeEmpty();

        product.ClearDomainEvents();

        product.DomainEvents.ShouldBeEmpty();
    }
}
