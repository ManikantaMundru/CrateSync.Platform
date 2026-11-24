using CrateSync.Platform.Inventory.Domain.Exceptions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.Stock.Events;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Inventory.Tests.Domain;

public sealed class InventoryBalanceTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid ProductVarietyId = Guid.NewGuid();

    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Inventory_WhenShipmentReceived_ShouldIncreaseOnHandAndCreateAuditMovement()
    {
        // Arrange
        var balance = CreateBalance();
        var shipmentId = Guid.NewGuid();

        // Act
        balance.Receive(
            new CrateQuantity(100),
            StockReference.Shipment(shipmentId),
            Now.AddMinutes(1));

        // Assert
        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(100);

        var movement = balance.Movements.Single();
        movement.Type.ShouldBe(StockMovementType.Received);
        movement.OnHandChange.ShouldBe(100);
        movement.Reference.Id.ShouldBe(shipmentId);

        balance.DomainEvents.ShouldContain(
            x => x is StockReceivedDomainEvent);
    }

    [Fact]
    public void Inventory_WhenTwoOrdersReserveStock_ShouldTrackReservationsPerOrder()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();

        // Act
        balance.ReserveForOrder(
            orderA,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        balance.ReserveForOrder(
            orderB,
            new CrateQuantity(20),
            Now.AddMinutes(3));

        // Assert
        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(50);
        balance.Available.ShouldBe(50);

        balance.Reservations.Single(x => x.SalesOrderId == orderA)
            .Quantity.ShouldBe(30);

        balance.Reservations.Single(x => x.SalesOrderId == orderB)
            .Quantity.ShouldBe(20);
    }

    [Fact]
    public void Inventory_WhenOrderAReleased_ShouldNotReleaseOrderBReservation()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();

        balance.ReserveForOrder(
            orderA,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        balance.ReserveForOrder(
            orderB,
            new CrateQuantity(20),
            Now.AddMinutes(3));

        // Act
        var changed = balance.ReleaseForOrder(
            orderA,
            Now.AddMinutes(4));

        // Assert
        changed.ShouldBeTrue();

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(20);
        balance.Available.ShouldBe(80);

        balance.Reservations.Single(x => x.SalesOrderId == orderA)
            .Status.ShouldBe(StockReservationStatus.Released);

        balance.Reservations.Single(x => x.SalesOrderId == orderB)
            .Status.ShouldBe(StockReservationStatus.Active);
    }

    [Fact]
    public void Inventory_WhenOrderDispatched_ShouldDecreaseBothOnHandAndReserved()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderId = Guid.NewGuid();

        balance.ReserveForOrder(
            orderId,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        // Act
        var changed = balance.DispatchForOrder(
            orderId,
            Now.AddMinutes(3));

        // Assert
        changed.ShouldBeTrue();

        balance.OnHand.Value.ShouldBe(70);
        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(70);

        balance.Reservations.Single()
            .Status.ShouldBe(StockReservationStatus.Dispatched);

        var movement = balance.Movements.Last();
        movement.Type.ShouldBe(StockMovementType.Dispatched);
        movement.OnHandChange.ShouldBe(-30);
        movement.ReservedChange.ShouldBe(-30);

        balance.DomainEvents.ShouldContain(
            x => x is StockDispatchedDomainEvent);
    }

    [Fact]
    public void Inventory_WhenDifferentOrderRequestsDispatch_ShouldNotConsumeOtherOrdersStock()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderA = Guid.NewGuid();
        var orderB = Guid.NewGuid();

        balance.ReserveForOrder(
            orderB,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        // Act
        var changed = balance.DispatchForOrder(
            orderA,
            Now.AddMinutes(3));

        // Assert
        changed.ShouldBeFalse();
        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(30);
        balance.Available.ShouldBe(70);

        balance.Reservations.Single()
            .Status.ShouldBe(StockReservationStatus.Active);
    }

    [Fact]
    public void Inventory_WhenSameReservationReplayed_ShouldBeIdempotent()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderId = Guid.NewGuid();

        var first = balance.ReserveForOrder(
            orderId,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        var movementCount = balance.Movements.Count;

        // Act
        var second = balance.ReserveForOrder(
            orderId,
            new CrateQuantity(30),
            Now.AddMinutes(3));

        // Assert
        first.ShouldBeTrue();
        second.ShouldBeFalse();

        balance.Reserved.Value.ShouldBe(30);
        balance.Movements.Count.ShouldBe(movementCount);
    }

    [Fact]
    public void Inventory_WhenSameOrderAttemptsDifferentReservation_ShouldRejectConflict()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderId = Guid.NewGuid();

        balance.ReserveForOrder(
            orderId,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => balance.ReserveForOrder(
                orderId,
                new CrateQuantity(40),
                Now.AddMinutes(3)));

        // Assert
        exception.Message.ShouldBe(
            "A reservation already exists for this sales order.");

        balance.Reserved.Value.ShouldBe(30);
    }

    [Fact]
    public void Inventory_WhenAvailableStockIsInsufficient_ShouldNotMutateReservationState()
    {
        // Arrange
        var balance = CreateBalanceWithStock(20);

        // Act
        Should.Throw<InsufficientStockException>(
            () => balance.ReserveForOrder(
                Guid.NewGuid(),
                new CrateQuantity(30),
                Now.AddMinutes(2)));

        // Assert
        balance.OnHand.Value.ShouldBe(20);
        balance.Reserved.Value.ShouldBe(0);
        balance.Reservations.ShouldBeEmpty();
    }

    [Fact]
    public void Inventory_WhenReleasedReservationIsDispatched_ShouldRejectJourney()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);
        var orderId = Guid.NewGuid();

        balance.ReserveForOrder(
            orderId,
            new CrateQuantity(30),
            Now.AddMinutes(2));

        balance.ReleaseForOrder(
            orderId,
            Now.AddMinutes(3));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => balance.DispatchForOrder(
                orderId,
                Now.AddMinutes(4)));

        // Assert
        exception.Message.ShouldBe(
            "Only an active reservation can be dispatched.");

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(0);
    }

    [Fact]
    public void Inventory_AdjustmentCannotReduceOnHandBelowReservedStock()
    {
        // Arrange
        var balance = CreateBalanceWithStock(100);

        balance.ReserveForOrder(
            Guid.NewGuid(),
            new CrateQuantity(60),
            Now.AddMinutes(2));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => balance.Adjust(
                -50,
                new AdjustmentReason("Stock count correction"),
                StockReference.Adjustment(Guid.NewGuid()),
                Now.AddMinutes(3)));

        // Assert
        exception.Message.ShouldBe(
            "Adjustment would reduce on-hand stock below reserved stock.");

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(60);
    }

    private static InventoryBalance CreateBalance()
    {
        return InventoryBalance.Create(
            TenantId,
            WarehouseId,
            ProductVarietyId,
            "ZA",
            Now);
    }

    private static InventoryBalance CreateBalanceWithStock(int quantity)
    {
        var balance = CreateBalance();

        balance.Receive(
            new CrateQuantity(quantity),
            StockReference.Shipment(Guid.NewGuid()),
            Now.AddMinutes(1));

        balance.ClearDomainEvents();

        return balance;
    }
    [Fact]
    public void Receive_Should_Not_Double_Count_Same_Shipment_Receipt()
    {
        var balance = InventoryBalance.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ZA",
            Now);

        var receiptId = Guid.NewGuid();
        var reference =
            StockReference.ShipmentReceipt(
                receiptId);

        var first = balance.Receive(
            new CrateQuantity(40),
            reference,
            Now);

        var second = balance.Receive(
            new CrateQuantity(40),
            reference,
            Now.AddMinutes(1));

        first.ShouldBeTrue();
        second.ShouldBeFalse();
        balance.OnHand.Value.ShouldBe(40);
        balance.Movements.Count.ShouldBe(1);
    }


    [Fact]
    public void TransferOut_Should_Use_Available_Not_OnHand()
    {
        var balance = CreateBalanceWithStock(100);
        balance.ReserveForOrder(
            Guid.NewGuid(),
            new CrateQuantity(80),
            Now.AddMinutes(2));

        var reference =
            StockReference.WarehouseTransfer(
                Guid.NewGuid());

        Should.Throw<InsufficientStockException>(
            () => balance.TransferOut(
                new CrateQuantity(30),
                reference,
                "TRF-001",
                Now.AddMinutes(3)));

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(80);
        balance.Available.ShouldBe(20);
    }

    [Fact]
    public void TransferOut_And_TransferIn_Should_Create_Linked_Movements()
    {
        var source = CreateBalanceWithStock(100);
        var destination = InventoryBalance.Create(
            TenantId,
            Guid.NewGuid(),
            ProductVarietyId,
            "ZA",
            Now);

        var transferId = Guid.NewGuid();
        var reference =
            StockReference.WarehouseTransfer(
                transferId);

        source.TransferOut(
            new CrateQuantity(40),
            reference,
            "TRF-001",
            Now.AddMinutes(2));

        destination.TransferIn(
            new CrateQuantity(40),
            reference,
            "TRF-001",
            Now.AddMinutes(2));

        source.OnHand.Value.ShouldBe(60);
        destination.OnHand.Value.ShouldBe(40);

        source.Movements.Last().Type.ShouldBe(
            StockMovementType.TransferOut);
        destination.Movements.Last().Type.ShouldBe(
            StockMovementType.TransferIn);

        source.Movements.Last().Reference.Id
            .ShouldBe(transferId);
        destination.Movements.Last().Reference.Id
            .ShouldBe(transferId);
    }

    [Fact]
    public void Transfer_Should_Be_Idempotent_For_Same_Transfer_Id()
    {
        var source = CreateBalanceWithStock(100);
        var destination = InventoryBalance.Create(
            TenantId,
            Guid.NewGuid(),
            ProductVarietyId,
            "ZA",
            Now);

        var reference =
            StockReference.WarehouseTransfer(
                Guid.NewGuid());

        source.TransferOut(
            new CrateQuantity(40),
            reference,
            null,
            Now.AddMinutes(2));

        destination.TransferIn(
            new CrateQuantity(40),
            reference,
            null,
            Now.AddMinutes(2));

        var sourceReplay = source.TransferOut(
            new CrateQuantity(40),
            reference,
            null,
            Now.AddMinutes(3));

        var destinationReplay =
            destination.TransferIn(
                new CrateQuantity(40),
                reference,
                null,
                Now.AddMinutes(3));

        sourceReplay.ShouldBeFalse();
        destinationReplay.ShouldBeFalse();
        source.OnHand.Value.ShouldBe(60);
        destination.OnHand.Value.ShouldBe(40);
    }


}
