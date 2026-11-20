using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.ReceiveStock;
using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.ReceiveStock;

public sealed class ReceiveStockCommandHandlerTests
{
    private readonly IInventoryBalanceRepository _repository =
        Substitute.For<IInventoryBalanceRepository>();

    private readonly IInventoryUnitOfWork _unitOfWork =
        Substitute.For<IInventoryUnitOfWork>();

    private readonly ITenantContext _tenantContext =
        Substitute.For<ITenantContext>();

    private readonly FakeTimeProvider _timeProvider =
        new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_Should_Create_Balance_When_It_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        _repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns((InventoryBalance?)null);

        var handler = new ReceiveStockCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var result = await handler.Handle(
            new ReceiveStockCommand(
                warehouseId,
                productVarietyId,
                100,
                shipmentId),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();

        _repository.Received(1)
            .Add(Arg.Is<InventoryBalance>(x =>
                x.TenantId == tenantId &&
                x.WarehouseId == warehouseId &&
                x.ProductVarietyId == productVarietyId &&
                x.OnHand.Value == 100));

        await _unitOfWork.Received(1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_Should_Update_Existing_Balance()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        var balance = InventoryBalance.Create(
            tenantId,
            warehouseId,
            productVarietyId,
            _timeProvider.GetUtcNow());

        _repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns(balance);

        var handler = new ReceiveStockCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var result = await handler.Handle(
            new ReceiveStockCommand(
                warehouseId,
                productVarietyId,
                50,
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        balance.OnHand.Value.ShouldBe(50);

        _repository.DidNotReceive()
            .Add(Arg.Any<InventoryBalance>());
    }
}
