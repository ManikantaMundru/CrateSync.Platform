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
        new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Should_Start_With_Zero_Stock()
    {
        var balance = CreateBalance();

        balance.TenantId.ShouldBe(TenantId);
        balance.WarehouseId.ShouldBe(WarehouseId);
        balance.ProductVarietyId.ShouldBe(ProductVarietyId);

        balance.OnHand.Value.ShouldBe(0);
        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(0);
        balance.Movements.ShouldBeEmpty();
    }

    [Fact]
    public void Receive_Should_Increase_OnHand()
    {
        var balance = CreateBalance();

        var shipmentId = Guid.NewGuid();

        balance.Receive(
            new CrateQuantity(100),
            StockReference.Shipment(shipmentId),
            Now.AddMinutes(1));

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(100);

        balance.Movements.Count.ShouldBe(1);

        var movement = balance.Movements.Single();

        movement.Type.ShouldBe(StockMovementType.Received);
        movement.OnHandChange.ShouldBe(100);
        movement.ReservedChange.ShouldBe(0);
        movement.OnHandAfter.ShouldBe(100);
        movement.ReservedAfter.ShouldBe(0);
        movement.Reference.Id.ShouldBe(shipmentId);

        balance.DomainEvents
            .ShouldContain(x => x is StockReceivedDomainEvent);
    }

    [Fact]
    public void Reserve_Should_Increase_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        var salesOrderId = Guid.NewGuid();

        balance.Reserve(
            new CrateQuantity(30),
            StockReference.SalesOrder(salesOrderId),
            Now.AddMinutes(2));

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(30);
        balance.Available.ShouldBe(70);

        var movement = balance.Movements.Last();

        movement.Type.ShouldBe(StockMovementType.Reserved);
        movement.OnHandChange.ShouldBe(0);
        movement.ReservedChange.ShouldBe(30);
        movement.ReservedAfter.ShouldBe(30);
    }

    [Fact]
    public void Reserve_Should_Throw_When_Available_Stock_Is_Insufficient()
    {
        var balance = CreateBalance();

        Receive(balance, 20);

        Should.Throw<InsufficientStockException>(() =>
            balance.Reserve(
                new CrateQuantity(30),
                StockReference.SalesOrder(Guid.NewGuid()),
                Now.AddMinutes(2)));

        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(20);
    }

    [Fact]
    public void ReleaseReservation_Should_Decrease_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        var orderId = Guid.NewGuid();

        balance.Reserve(
            new CrateQuantity(40),
            StockReference.SalesOrder(orderId),
            Now.AddMinutes(2));

        balance.ReleaseReservation(
            new CrateQuantity(15),
            StockReference.SalesOrder(orderId),
            Now.AddMinutes(3));

        balance.OnHand.Value.ShouldBe(100);
        balance.Reserved.Value.ShouldBe(25);
        balance.Available.ShouldBe(75);

        var movement = balance.Movements.Last();

        movement.Type.ShouldBe(
            StockMovementType.ReservationReleased);

        movement.ReservedChange.ShouldBe(-15);
    }

    [Fact]
    public void ReleaseReservation_Should_Throw_When_Quantity_Exceeds_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        balance.Reserve(
            new CrateQuantity(20),
            StockReference.SalesOrder(Guid.NewGuid()),
            Now.AddMinutes(2));

        Should.Throw<InsufficientReservedStockException>(() =>
            balance.ReleaseReservation(
                new CrateQuantity(21),
                StockReference.SalesOrder(Guid.NewGuid()),
                Now.AddMinutes(3)));
    }

    [Fact]
    public void Dispatch_Should_Decrease_OnHand_And_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        var orderId = Guid.NewGuid();

        balance.Reserve(
            new CrateQuantity(30),
            StockReference.SalesOrder(orderId),
            Now.AddMinutes(2));

        balance.Dispatch(
            new CrateQuantity(30),
            StockReference.SalesOrder(orderId),
            Now.AddMinutes(3));

        balance.OnHand.Value.ShouldBe(70);
        balance.Reserved.Value.ShouldBe(0);
        balance.Available.ShouldBe(70);

        var movement = balance.Movements.Last();

        movement.Type.ShouldBe(
            StockMovementType.Dispatched);

        movement.OnHandChange.ShouldBe(-30);
        movement.ReservedChange.ShouldBe(-30);

        balance.DomainEvents
            .ShouldContain(x => x is StockDispatchedDomainEvent);
    }

    [Fact]
    public void Dispatch_Should_Throw_When_Quantity_Exceeds_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        balance.Reserve(
            new CrateQuantity(20),
            StockReference.SalesOrder(Guid.NewGuid()),
            Now.AddMinutes(2));

        Should.Throw<InsufficientReservedStockException>(() =>
            balance.Dispatch(
                new CrateQuantity(30),
                StockReference.SalesOrder(Guid.NewGuid()),
                Now.AddMinutes(3)));
    }

    [Fact]
    public void Positive_Adjustment_Should_Increase_OnHand()
    {
        var balance = CreateBalance();

        Receive(balance, 50);

        balance.Adjust(
            10,
            new AdjustmentReason("Stock count correction"),
            StockReference.Adjustment(Guid.NewGuid()),
            Now.AddMinutes(2));

        balance.OnHand.Value.ShouldBe(60);
        balance.Available.ShouldBe(60);

        var movement = balance.Movements.Last();

        movement.Type.ShouldBe(StockMovementType.Adjusted);
        movement.OnHandChange.ShouldBe(10);
        movement.Reason.ShouldBe("Stock count correction");
    }

    [Fact]
    public void Negative_Adjustment_Should_Decrease_OnHand()
    {
        var balance = CreateBalance();

        Receive(balance, 50);

        balance.Adjust(
            -10,
            new AdjustmentReason("Damaged crates"),
            StockReference.Adjustment(Guid.NewGuid()),
            Now.AddMinutes(2));

        balance.OnHand.Value.ShouldBe(40);
    }

    [Fact]
    public void Adjustment_Should_Not_Allow_Negative_OnHand()
    {
        var balance = CreateBalance();

        Receive(balance, 10);

        Should.Throw<InsufficientStockException>(() =>
            balance.Adjust(
                -11,
                new AdjustmentReason("Correction"),
                StockReference.Adjustment(Guid.NewGuid()),
                Now.AddMinutes(2)));
    }

    [Fact]
    public void Adjustment_Should_Not_Reduce_OnHand_Below_Reserved()
    {
        var balance = CreateBalance();

        Receive(balance, 100);

        balance.Reserve(
            new CrateQuantity(80),
            StockReference.SalesOrder(Guid.NewGuid()),
            Now.AddMinutes(2));

        Should.Throw<InvalidOperationException>(() =>
            balance.Adjust(
                -30,
                new AdjustmentReason("Damaged stock"),
                StockReference.Adjustment(Guid.NewGuid()),
                Now.AddMinutes(3)));
    }

    [Fact]
    public void Adjustment_Should_Reject_Zero_Quantity()
    {
        var balance = CreateBalance();

        Should.Throw<ArgumentException>(() =>
            balance.Adjust(
                0,
                new AdjustmentReason("Correction"),
                StockReference.Adjustment(Guid.NewGuid()),
                Now));
    }

    private static InventoryBalance CreateBalance()
    {
        return InventoryBalance.Create(
            TenantId,
            WarehouseId,
            ProductVarietyId,
            Now);
    }

    private static void Receive(
        InventoryBalance balance,
        int quantity)
    {
        balance.Receive(
            new CrateQuantity(quantity),
            StockReference.Shipment(Guid.NewGuid()),
            Now.AddMinutes(1));
    }
}
