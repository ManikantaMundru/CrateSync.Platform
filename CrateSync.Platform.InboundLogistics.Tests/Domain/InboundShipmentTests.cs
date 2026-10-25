using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.InboundLogistics.Tests.Domain;

public sealed class InboundShipmentTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    private static readonly DateTimeOffset Now =
        new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Should_Create_Draft_Shipment()
    {
        var shipment = CreateShipment();

        shipment.TenantId.ShouldBe(TenantId);
        shipment.SupplierId.ShouldBe(SupplierId);
        shipment.WarehouseId.ShouldBe(WarehouseId);
        shipment.Status.ShouldBe(ShipmentStatus.Draft);
        shipment.CreatedAtUtc.ShouldBe(Now);
        shipment.UpdatedAtUtc.ShouldBe(Now);
        shipment.Lines.ShouldBeEmpty();
    }

    [Fact]
    public void Create_Should_Raise_ShipmentCreatedDomainEvent()
    {
        var shipment = CreateShipment();

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentCreatedDomainEvent);
    }

    [Fact]
    public void AddLine_Should_Add_Line_To_Draft_Shipment()
    {
        var shipment = CreateShipment();

        var lineId = AddLine(shipment);

        lineId.Value.ShouldNotBe(Guid.Empty);
        shipment.Lines.Count.ShouldBe(1);

        var line = shipment.Lines.Single();

        line.ProductId.ShouldNotBe(Guid.Empty);
        line.ProductVarietyId.ShouldNotBe(Guid.Empty);
        line.BatchCode.Value.ShouldBe("BATCH-001");
        line.ExpectedQuantity.Value.ShouldBe(100);
        line.ReceivedQuantity.Value.ShouldBe(0);
        line.PackSize.WeightKg.ShouldBe(10m);
        line.UnitCost.Amount.ShouldBe(125m);
        line.UnitCost.Currency.Code.ShouldBe("ZAR");
    }

    [Fact]
    public void AddLine_Should_Reject_Duplicate_Batch_Code()
    {
        var shipment = CreateShipment();

        AddLine(shipment);

        Should.Throw<InvalidOperationException>(() =>
            AddLine(shipment));
    }

    [Fact]
    public void MarkShipped_Should_Require_At_Least_One_Line()
    {
        var shipment = CreateShipment();

        Should.Throw<InvalidOperationException>(() =>
            shipment.MarkShipped(Now.AddHours(1)));
    }

    [Fact]
    public void MarkShipped_Should_Change_Status()
    {
        var shipment = CreateShipment();
        AddLine(shipment);

        var shippedAt = Now.AddHours(1);

        shipment.MarkShipped(shippedAt);

        shipment.Status.ShouldBe(ShipmentStatus.Shipped);
        shipment.ShippedAtUtc.ShouldBe(shippedAt);
        shipment.UpdatedAtUtc.ShouldBe(shippedAt);

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentShippedDomainEvent);
    }

    [Fact]
    public void AddLine_Should_Not_Be_Allowed_After_Shipment_Is_Shipped()
    {
        var shipment = CreateShipment();

        AddLine(shipment);

        shipment.MarkShipped(Now.AddHours(1));

        Should.Throw<InvalidOperationException>(() =>
            AddLine(shipment));
    }

    [Fact]
    public void MarkArrived_Should_Change_Status()
    {
        var shipment = CreateShippedShipment();

        var arrivedAt = Now.AddDays(1);

        shipment.MarkArrived(arrivedAt);

        shipment.Status.ShouldBe(ShipmentStatus.Arrived);
        shipment.ArrivedAtUtc.ShouldBe(arrivedAt);

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentArrivedDomainEvent);
    }

    [Fact]
    public void MarkArrived_Should_Only_Work_For_Shipped_Shipment()
    {
        var shipment = CreateShipment();

        Should.Throw<InvalidOperationException>(() =>
            shipment.MarkArrived(Now.AddDays(1)));
    }

    [Fact]
    public void ReceiveLine_Should_Partially_Receive_Shipment()
    {
        var shipment = CreateArrivedShipment();

        var line = shipment.Lines.Single();

        shipment.ReceiveLine(
            line.Id,
            new CrateQuantity(40),
            Now.AddDays(2));

        shipment.Status.ShouldBe(ShipmentStatus.PartiallyReceived);
        line.ReceivedQuantity.Value.ShouldBe(40);

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentPartiallyReceivedDomainEvent);
    }

    [Fact]
    public void ReceiveLine_Should_Mark_Shipment_Received_When_All_Lines_Are_Complete()
    {
        var shipment = CreateArrivedShipment();

        var line = shipment.Lines.Single();
        var receivedAt = Now.AddDays(2);

        shipment.ReceiveLine(
            line.Id,
            new CrateQuantity(100),
            receivedAt);

        shipment.Status.ShouldBe(ShipmentStatus.Received);
        shipment.ReceivedAtUtc.ShouldBe(receivedAt);
        line.ReceivedQuantity.Value.ShouldBe(100);

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentReceivedDomainEvent);
    }

    [Fact]
    public void ReceiveLine_Should_Support_Multiple_Partial_Receipts()
    {
        var shipment = CreateArrivedShipment();

        var line = shipment.Lines.Single();

        shipment.ReceiveLine(
            line.Id,
            new CrateQuantity(40),
            Now.AddDays(2));

        shipment.ReceiveLine(
            line.Id,
            new CrateQuantity(60),
            Now.AddDays(3));

        line.ReceivedQuantity.Value.ShouldBe(100);
        shipment.Status.ShouldBe(ShipmentStatus.Received);
    }

    [Fact]
    public void ReceiveLine_Should_Not_Allow_Quantity_Above_Expected()
    {
        var shipment = CreateArrivedShipment();

        var line = shipment.Lines.Single();

        Should.Throw<InvalidOperationException>(() =>
            shipment.ReceiveLine(
                line.Id,
                new CrateQuantity(101),
                Now.AddDays(2)));
    }

    [Fact]
    public void Cancel_Should_Cancel_Draft_Shipment()
    {
        var shipment = CreateShipment();

        var cancelledAt = Now.AddHours(1);

        shipment.Cancel(cancelledAt);

        shipment.Status.ShouldBe(ShipmentStatus.Cancelled);
        shipment.CancelledAtUtc.ShouldBe(cancelledAt);

        shipment.DomainEvents
            .ShouldContain(x => x is ShipmentCancelledDomainEvent);
    }

    [Fact]
    public void Cancel_Should_Not_Be_Allowed_After_Receiving_Starts()
    {
        var shipment = CreateArrivedShipment();

        var line = shipment.Lines.Single();

        shipment.ReceiveLine(
            line.Id,
            new CrateQuantity(10),
            Now.AddDays(2));

        Should.Throw<InvalidOperationException>(() =>
            shipment.Cancel(Now.AddDays(3)));
    }

    private static InboundShipment CreateShipment()
    {
        return InboundShipment.Create(
            TenantId,
            SupplierId,
            WarehouseId,
            new TrackingNumber("TRACK-001"),
            new CountryCode("ZA"),
            Now.AddDays(5),
            Now);
    }

    private static ShipmentLineId AddLine(InboundShipment shipment)
    {
        return shipment.AddLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new BatchCode("batch-001"),
            new CrateQuantity(100),
            new PackSize(10m),
            new Money(125m, new Currency("ZAR")),
            Now.AddMinutes(10));
    }

    private static InboundShipment CreateShippedShipment()
    {
        var shipment = CreateShipment();

        AddLine(shipment);

        shipment.MarkShipped(Now.AddHours(1));

        return shipment;
    }

    private static InboundShipment CreateArrivedShipment()
    {
        var shipment = CreateShippedShipment();

        shipment.MarkArrived(Now.AddDays(1));

        return shipment;
    }
}
