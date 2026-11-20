using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.ReserveStock;

public sealed class ReserveStockCommandHandlerTests
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
    public async Task Handle_Should_Reserve_Available_Stock()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        var balance = CreateBalance(
            tenantId,
            warehouseId,
            productVarietyId);

        balance.Receive(
            new CrateQuantity(100),
            StockReference.Shipment(Guid.NewGuid()),
            _timeProvider.GetUtcNow());

        _repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns(balance);

        var handler = new ReserveStockCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var result = await handler.Handle(
            new ReserveStockCommand(
                warehouseId,
                productVarietyId,
                30,
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        balance.Reserved.Value.ShouldBe(30);
        balance.Available.ShouldBe(70);

        await _unitOfWork.Received(1)
            .SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Balance_Does_Not_Exist()
    {
        var tenantId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        _repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns((InventoryBalance?)null);

        var handler = new ReserveStockCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var result = await handler.Handle(
            new ReserveStockCommand(
                warehouseId,
                productVarietyId,
                10,
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Stock_Is_Insufficient()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var productVarietyId = Guid.NewGuid();

        _tenantContext.TenantId.Returns(tenantId);

        var balance = CreateBalance(
            tenantId,
            warehouseId,
            productVarietyId);

        balance.Receive(
            new CrateQuantity(10),
            StockReference.Shipment(Guid.NewGuid()),
            _timeProvider.GetUtcNow());

        _repository
            .GetAsync(
                tenantId,
                warehouseId,
                productVarietyId,
                TestContext.Current.CancellationToken)
            .Returns(balance);

        var handler = new ReserveStockCommandHandler(
            _repository,
            _unitOfWork,
            _tenantContext,
            _timeProvider);

        var result = await handler.Handle(
            new ReserveStockCommand(
                warehouseId,
                productVarietyId,
                20,
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        balance.Reserved.Value.ShouldBe(0);

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private InventoryBalance CreateBalance(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId)
    {
        return InventoryBalance.Create(
            tenantId,
            warehouseId,
            productVarietyId,
            _timeProvider.GetUtcNow());
    }
}
