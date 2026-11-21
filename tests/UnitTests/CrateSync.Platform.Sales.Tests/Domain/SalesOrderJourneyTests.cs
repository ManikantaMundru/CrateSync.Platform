using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using CrateSync.Platform.Sales.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Sales.Tests.Domain;

public sealed class SalesOrderJourneyTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SalesOrder_WithValidLine_WhenConfirmed_ShouldMoveFromDraftToConfirmed()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");

        // Act
        order.Confirm(Now.AddMinutes(1));

        // Assert
        order.Status.ShouldBe(SalesOrderStatus.Confirmed);
        order.ConfirmedAtUtc.ShouldBe(Now.AddMinutes(1));
        order.DomainEvents.ShouldContain(x => x is SalesOrderConfirmedDomainEvent);
    }

    [Fact]
    public void SalesOrder_WithoutLines_WhenConfirmed_ShouldRejectJourney()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => order.Confirm(Now.AddMinutes(1)));

        // Assert
        exception.Message.ShouldBe("Sales order must contain at least one line.");
        order.Status.ShouldBe(SalesOrderStatus.Draft);
    }

    [Fact]
    public void SalesOrder_WithMixedCurrencies_WhenAddingSecondLine_ShouldRejectJourney()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => AddLine(order, Guid.NewGuid(), 5, "USD"));

        // Assert
        exception.Message.ShouldBe("All order lines must use the same currency.");
        order.Lines.Count.ShouldBe(1);
    }

    [Fact]
    public void SalesOrder_AfterConfirmation_WhenEdited_ShouldRejectJourney()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => AddLine(order, Guid.NewGuid(), 5, "ZAR"));

        // Assert
        exception.Message.ShouldBe("Only draft sales orders can be edited.");
        order.Lines.Count.ShouldBe(1);
    }

    [Fact]
    public void SalesOrder_WhenReservationFails_ShouldRemainConfirmedAndRecordReason()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));

        // Act
        order.RecordReservationFailure(
            "Insufficient stock for product variety.",
            Now.AddMinutes(2));

        // Assert
        order.Status.ShouldBe(SalesOrderStatus.Confirmed);
        order.ReservationFailureReason.ShouldBe(
            "Insufficient stock for product variety.");
    }

    [Fact]
    public void SalesOrder_WhenStockIsReserved_ShouldProgressThroughAllocatedDispatchAndFulfilled()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));

        // Act
        order.Allocate(Now.AddMinutes(2));
        order.RequestDispatch(Now.AddMinutes(3));
        order.Dispatch(Now.AddMinutes(4));
        order.Fulfil(Now.AddMinutes(5));

        // Assert
        order.Status.ShouldBe(SalesOrderStatus.Fulfilled);
        order.AllocatedAtUtc.ShouldBe(Now.AddMinutes(2));
        order.DispatchedAtUtc.ShouldBe(Now.AddMinutes(4));
        order.FulfilledAtUtc.ShouldBe(Now.AddMinutes(5));

        order.DomainEvents.ShouldContain(
            x => x is SalesOrderAllocatedDomainEvent);

        order.DomainEvents.ShouldContain(
            x => x is SalesOrderDispatchRequestedDomainEvent);

        order.DomainEvents.ShouldContain(
            x => x is SalesOrderDispatchedDomainEvent);

        order.DomainEvents.ShouldContain(
            x => x is SalesOrderFulfilledDomainEvent);
    }

    [Fact]
    public void SalesOrder_BeforeAllocation_WhenDispatchRequested_ShouldRejectJourney()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => order.RequestDispatch(Now.AddMinutes(2)));

        // Assert
        exception.Message.ShouldBe(
            "Only allocated sales orders can request dispatch.");

        order.Status.ShouldBe(SalesOrderStatus.Confirmed);
    }

    [Fact]
    public void SalesOrder_WhenAllocatedAndCancelled_ShouldBecomeCancelled()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));
        order.Allocate(Now.AddMinutes(2));

        // Act
        order.Cancel(Now.AddMinutes(3));

        // Assert
        order.Status.ShouldBe(SalesOrderStatus.Cancelled);
        order.CancelledAtUtc.ShouldBe(Now.AddMinutes(3));
        order.DomainEvents.ShouldContain(
            x => x is SalesOrderCancelledDomainEvent);
    }

    [Fact]
    public void SalesOrder_AfterDispatch_WhenCancelled_ShouldRejectJourney()
    {
        // Arrange
        var order = CreateOrder();
        AddLine(order, Guid.NewGuid(), 10, "ZAR");
        order.Confirm(Now.AddMinutes(1));
        order.Allocate(Now.AddMinutes(2));
        order.Dispatch(Now.AddMinutes(3));

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => order.Cancel(Now.AddMinutes(4)));

        // Assert
        exception.Message.ShouldBe(
            "A dispatched or fulfilled order cannot be cancelled.");

        order.Status.ShouldBe(SalesOrderStatus.Dispatched);
    }

    [Fact]
    public void SalesOrder_WhenSameProductAddedTwice_ShouldRejectDuplicateLine()
    {
        // Arrange
        var order = CreateOrder();
        var productVarietyId = Guid.NewGuid();
        AddLine(order, productVarietyId, 10, "ZAR");

        // Act
        var exception = Should.Throw<InvalidOperationException>(
            () => AddLine(order, productVarietyId, 5, "ZAR"));

        // Assert
        exception.Message.ShouldBe(
            "Product variety already exists on this order.");

        order.Lines.Count.ShouldBe(1);
    }

    private static SalesOrder CreateOrder()
    {
        return SalesOrder.Create(
            TenantId,
            CustomerId,
            WarehouseId,
            new OrderNumber("SO-TEST-001"),
            new DeliveryAddress(
                "1 Test Street",
                null,
                "Cape Town",
                "Western Cape",
                "7441",
                "South Africa"),
            Now);
    }

    private static SalesOrderLine AddLine(
        SalesOrder order,
        Guid productVarietyId,
        int quantity,
        string currency)
    {
        return order.AddLine(
            productVarietyId,
            new CrateQuantity(quantity),
            new PackSize(18m),
            new Money(250m, new Currency(currency)),
            Now);
    }
}
