using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.GetStock;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.GetStock;

public sealed class GetStockQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Stock_When_Found()
    {
        var readService = Substitute.For<IInventoryReadService>();
        var tenantContext = Substitute.For<ITenantContext>();

        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        tenantContext.TenantId.Returns(tenantId);

        var response = new StockResponse(
            Guid.NewGuid(),
            warehouseId,
            productVarietyId,
            100,
            30,
            70,
            DateTimeOffset.UtcNow);

        readService
            .GetStockAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns(response);

        var handler = new GetStockQueryHandler(
            readService,
            tenantContext);

        var result = await handler.Handle(
            new GetStockQuery(
                warehouseId,
                productVarietyId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(response);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Not_Found()
    {
        var readService = Substitute.For<IInventoryReadService>();
        var tenantContext = Substitute.For<ITenantContext>();

        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        tenantContext.TenantId.Returns(tenantId);

        readService
            .GetStockAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns((StockResponse?)null);

        var handler = new GetStockQueryHandler(
            readService,
            tenantContext);

        var result = await handler.Handle(
            new GetStockQuery(
                warehouseId,
                productVarietyId),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
    }
}
