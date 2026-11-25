using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.IntegrationContracts.Sales;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.EventHandlers;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Application.EventHandlers;

public sealed class SalesOrderConfirmedIntegrationEventHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ConfirmedOrder_WhenEveryLineHasStock_ShouldReserveAllLinesAndPublishSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();
        var varietyA = Guid.NewGuid();
        var varietyB = Guid.NewGuid();

        var balanceA = CreateBalanceWithStock(
            tenantId, warehouseId, varietyA, 100);

        var balanceB = CreateBalanceWithStock(
            tenantId, warehouseId, varietyB, 50);

        var repository =
            Substitute.For<IInventoryBalanceRepository>();

        repository.GetBatchAsync(
                tenantId,
                warehouseId,
                varietyA,
                "ZA", Guid.Empty,
                Arg.Any<CancellationToken>())
            .Returns(balanceA);

        repository.GetBatchAsync(
                tenantId,
                warehouseId,
                varietyB,
                "ZA", Guid.Empty,
                Arg.Any<CancellationToken>())
            .Returns(balanceB);

        var unitOfWork =
            Substitute.For<IInventoryUnitOfWork>();

        var publisher =
            Substitute.For<IPublisher>();

        var timeProvider = new FakeTimeProvider(Now);

        var handler =
            new SalesOrderConfirmedIntegrationEventHandler(
                repository,
                unitOfWork,
                timeProvider,
                publisher);

        var notification =
            new SalesOrderConfirmedIntegrationEvent(
                tenantId,
                salesOrderId,
                warehouseId,
                [
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyA,
                        30,
                        "ZA"),
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyB,
                        20,
                        "ZA")
                ],
                Now);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        balanceA.Reserved.Value.ShouldBe(30);
        balanceB.Reserved.Value.ShouldBe(20);

        await unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());

        await publisher.Received(1).Publish(
            Arg.Is<StockReservedIntegrationEvent>(
                x =>
                    x.TenantId == tenantId &&
                    x.SalesOrderId == salesOrderId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmedOrder_WhenOneLineIsInsufficient_ShouldReserveNothingAndPublishFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();
        var varietyA = Guid.NewGuid();
        var varietyB = Guid.NewGuid();

        var balanceA = CreateBalanceWithStock(
            tenantId, warehouseId, varietyA, 100);

        var balanceB = CreateBalanceWithStock(
            tenantId, warehouseId, varietyB, 5);

        var repository =
            Substitute.For<IInventoryBalanceRepository>();

        repository.GetBatchAsync(
                tenantId,
                warehouseId,
                varietyA,
                "ZA", Guid.Empty,
                Arg.Any<CancellationToken>())
            .Returns(balanceA);

        repository.GetBatchAsync(
                tenantId,
                warehouseId,
                varietyB,
                "ZA", Guid.Empty,
                Arg.Any<CancellationToken>())
            .Returns(balanceB);

        var unitOfWork =
            Substitute.For<IInventoryUnitOfWork>();

        var publisher =
            Substitute.For<IPublisher>();

        var handler =
            new SalesOrderConfirmedIntegrationEventHandler(
                repository,
                unitOfWork,
                new FakeTimeProvider(Now),
                publisher);

        var notification =
            new SalesOrderConfirmedIntegrationEvent(
                tenantId,
                salesOrderId,
                warehouseId,
                [
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyA,
                        30,
                        "ZA"),
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyB,
                        20,
                        "ZA")
                ],
                Now);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        balanceA.Reserved.Value.ShouldBe(0);
        balanceB.Reserved.Value.ShouldBe(0);

        await unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());

        await publisher.Received(1).Publish(
            Arg.Is<StockReservationFailedIntegrationEvent>(
                x =>
                    x.TenantId == tenantId &&
                    x.SalesOrderId == salesOrderId &&
                    x.Reason.Contains("Insufficient stock")),
            Arg.Any<CancellationToken>());

        await publisher.DidNotReceive().Publish(
            Arg.Any<StockReservedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmedOrder_ReservesOnlyRequestedBatch()
    {
        var tenant = Guid.NewGuid();
        var warehouse = Guid.NewGuid();
        var variety = Guid.NewGuid();
        var batch = Guid.NewGuid();
        var balance = InventoryBalance.Create(tenant, warehouse, variety, "ZA", Now, batch, "BATCH-A", 18m);
        balance.Receive(new CrateQuantity(100), StockReference.Shipment(Guid.NewGuid()), Now);
        var repository = Substitute.For<IInventoryBalanceRepository>();
        repository.GetBatchAsync(tenant, warehouse, variety, "ZA", batch, Arg.Any<CancellationToken>()).Returns(balance);
        var handler = new SalesOrderConfirmedIntegrationEventHandler(repository, Substitute.For<IInventoryUnitOfWork>(), new FakeTimeProvider(Now), Substitute.For<IPublisher>());
        await handler.Handle(new SalesOrderConfirmedIntegrationEvent(tenant, Guid.NewGuid(), warehouse,
            [new SalesOrderLineIntegrationContract(Guid.NewGuid(), variety, 30, "ZA", batch)], Now), CancellationToken.None);
        balance.Reserved.Value.ShouldBe(30);
    }

    private static InventoryBalance CreateBalanceWithStock(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        int quantity)
    {
        var balance = InventoryBalance.Create(
            tenantId,
            warehouseId,
            productVarietyId,
            "ZA",
            Now.AddHours(-1));

        balance.Receive(
            new CrateQuantity(quantity),
            StockReference.Shipment(Guid.NewGuid()),
            Now.AddMinutes(-30));

        balance.ClearDomainEvents();

        return balance;
    }
}
