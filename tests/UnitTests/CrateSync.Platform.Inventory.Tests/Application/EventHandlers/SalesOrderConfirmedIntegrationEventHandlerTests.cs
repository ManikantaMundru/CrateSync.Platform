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

        repository.GetByVarietyAsync(
                tenantId,
                warehouseId,
                varietyA,
                Arg.Any<CancellationToken>())
            .Returns([balanceA]);

        repository.GetByVarietyAsync(
                tenantId,
                warehouseId,
                varietyB,
                Arg.Any<CancellationToken>())
            .Returns([balanceB]);

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
                        30),
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyB,
                        20)
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

        repository.GetByVarietyAsync(
                tenantId,
                warehouseId,
                varietyA,
                Arg.Any<CancellationToken>())
            .Returns([balanceA]);

        repository.GetByVarietyAsync(
                tenantId,
                warehouseId,
                varietyB,
                Arg.Any<CancellationToken>())
            .Returns([balanceB]);

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
                        30),
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyB,
                        20)
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
    public async Task ConfirmedOrder_WhenMultipleOriginsExist_ShouldFailInsteadOfChoosingArbitrarily()
    {
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();
        var varietyId = Guid.NewGuid();

        var southAfrica = CreateBalanceWithStock(
            tenantId,
            warehouseId,
            varietyId,
            100,
            "ZA");

        var italy = CreateBalanceWithStock(
            tenantId,
            warehouseId,
            varietyId,
            80,
            "IT");

        var repository =
            Substitute.For<IInventoryBalanceRepository>();

        repository.GetByVarietyAsync(
                tenantId,
                warehouseId,
                varietyId,
                Arg.Any<CancellationToken>())
            .Returns([southAfrica, italy]);

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

        await handler.Handle(
            new SalesOrderConfirmedIntegrationEvent(
                tenantId,
                salesOrderId,
                warehouseId,
                [
                    new SalesOrderLineIntegrationContract(
                        Guid.NewGuid(),
                        varietyId,
                        30)
                ],
                Now),
            CancellationToken.None);

        southAfrica.Reserved.Value.ShouldBe(0);
        italy.Reserved.Value.ShouldBe(0);

        await unitOfWork.DidNotReceive()
            .SaveChangesAsync(
                Arg.Any<CancellationToken>());

        await publisher.Received(1).Publish(
            Arg.Is<StockReservationFailedIntegrationEvent>(
                x =>
                    x.Reason.Contains(
                        "Multiple countries of origin")),
            Arg.Any<CancellationToken>());
    }

    private static InventoryBalance CreateBalanceWithStock(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        int quantity,
        string countryOfOrigin = "ZA")
    {
        var balance = InventoryBalance.Create(
            tenantId,
            warehouseId,
            productVarietyId,
            countryOfOrigin,
            Now.AddHours(-1));

        balance.Receive(
            new CrateQuantity(quantity),
            StockReference.Shipment(Guid.NewGuid()),
            Now.AddMinutes(-30));

        balance.ClearDomainEvents();

        return balance;
    }
}
