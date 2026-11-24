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

public sealed class SalesOrderCancellationAndDispatchIntegrationEventHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CancelledOrder_ShouldReleaseOnlyReservationsForThatOrder()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();

        var balanceA = CreateBalance(
            tenantId,
            warehouseId,
            Guid.NewGuid(),
            100);

        var balanceB = CreateBalance(
            tenantId,
            warehouseId,
            Guid.NewGuid(),
            100);

        balanceA.ReserveForOrder(
            orderA,
            new CrateQuantity(30),
            Now.AddMinutes(-10));

        balanceB.ReserveForOrder(
            orderB,
            new CrateQuantity(20),
            Now.AddMinutes(-10));

        var repository =
            Substitute.For<IInventoryBalanceRepository>();

        repository.GetBySalesOrderIdAsync(
                tenantId,
                orderA,
                Arg.Any<CancellationToken>())
            .Returns([balanceA]);

        var unitOfWork =
            Substitute.For<IInventoryUnitOfWork>();

        var handler =
            new SalesOrderCancelledIntegrationEventHandler(
                repository,
                unitOfWork,
                new FakeTimeProvider(Now));

        // Act
        await handler.Handle(
            new SalesOrderCancelledIntegrationEvent(
                tenantId,
                orderA,
                Now),
            CancellationToken.None);

        // Assert
        balanceA.Reserved.Value.ShouldBe(0);
        balanceA.Reservations.Single()
            .Status.ShouldBe(StockReservationStatus.Released);

        balanceB.Reserved.Value.ShouldBe(20);
        balanceB.Reservations.Single()
            .Status.ShouldBe(StockReservationStatus.Active);

        await unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchRequested_ShouldDispatchEveryReservationAndPublishSingleOutcome()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        var salesOrderId = Guid.NewGuid();

        var balanceA = CreateBalance(
            tenantId,
            warehouseId,
            Guid.NewGuid(),
            100);

        var balanceB = CreateBalance(
            tenantId,
            warehouseId,
            Guid.NewGuid(),
            50);

        balanceA.ReserveForOrder(
            salesOrderId,
            new CrateQuantity(30),
            Now.AddMinutes(-10));

        balanceB.ReserveForOrder(
            salesOrderId,
            new CrateQuantity(20),
            Now.AddMinutes(-10));

        var repository =
            Substitute.For<IInventoryBalanceRepository>();

        repository.GetBySalesOrderIdAsync(
                tenantId,
                salesOrderId,
                Arg.Any<CancellationToken>())
            .Returns([balanceA, balanceB]);

        var unitOfWork =
            Substitute.For<IInventoryUnitOfWork>();

        var publisher =
            Substitute.For<IPublisher>();

        var handler =
            new SalesOrderDispatchRequestedIntegrationEventHandler(
                repository,
                unitOfWork,
                new FakeTimeProvider(Now),
                publisher);

        // Act
        await handler.Handle(
            new SalesOrderDispatchRequestedIntegrationEvent(
                tenantId,
                salesOrderId,
                Now),
            CancellationToken.None);

        // Assert
        balanceA.OnHand.Value.ShouldBe(70);
        balanceA.Reserved.Value.ShouldBe(0);

        balanceB.OnHand.Value.ShouldBe(30);
        balanceB.Reserved.Value.ShouldBe(0);

        await unitOfWork.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());

        await publisher.Received(1).Publish(
            Arg.Is<StockDispatchedIntegrationEvent>(
                x =>
                    x.TenantId == tenantId &&
                    x.SalesOrderId == salesOrderId),
            Arg.Any<CancellationToken>());
    }

    private static InventoryBalance CreateBalance(
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
