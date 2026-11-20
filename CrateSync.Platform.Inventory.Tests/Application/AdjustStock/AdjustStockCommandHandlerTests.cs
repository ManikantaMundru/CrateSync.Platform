using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.AdjustStock;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.AdjustStock;

public sealed class AdjustStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Adjust_Stock()
    {
        var repository = Substitute.For<IInventoryBalanceRepository>();
        var unitOfWork = Substitute.For<IInventoryUnitOfWork>();
        var tenantContext = Substitute.For<ITenantContext>();

        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        tenantContext.TenantId.Returns(tenantId);

        var balance = InventoryBalance.Create(
            tenantId,
            warehouseId,
            productVarietyId,
            timeProvider.GetUtcNow());

        balance.Receive(
            new CrateQuantity(50),
            StockReference.Shipment(Guid.NewGuid()),
            timeProvider.GetUtcNow());

        repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns(balance);

        var handler = new AdjustStockCommandHandler(
            repository,
            unitOfWork,
            tenantContext,
            timeProvider);

        var result = await handler.Handle(
            new AdjustStockCommand(
                warehouseId,
                productVarietyId,
                -5,
                "Damaged crates"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        balance.OnHand.Value.ShouldBe(45);

        await unitOfWork.Received(1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
