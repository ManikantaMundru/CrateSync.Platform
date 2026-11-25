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
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
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
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
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
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
            new CrateQuantity(40),
            Now.AddDays(2));

        shipment.ReceiveLine(
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
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
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
                new CrateQuantity(101),
                Now.AddDays(2)));
    }

    [Fact]
    public void ReceiveLine_Should_Be_Idempotent_For_Same_Receipt_Id()
    {
        var shipment = CreateArrivedShipment();
        var line = shipment.Lines.Single();
        var receiptId = ShipmentReceiptId.New();

        shipment.ReceiveLine(
            receiptId,
            line.Id,
            WarehouseId,
            new CrateQuantity(40),
            Now.AddDays(2));

        shipment.ReceiveLine(
            receiptId,
            line.Id,
            WarehouseId,
            new CrateQuantity(40),
            Now.AddDays(2));

        line.ReceivedQuantity.Value.ShouldBe(40);
        shipment.Receipts.Count.ShouldBe(1);
    }

    [Fact]
    public void ReceiveLine_Should_Reject_Reused_Receipt_Id_With_Different_Quantity()
    {
        var shipment = CreateArrivedShipment();
        var line = shipment.Lines.Single();
        var receiptId = ShipmentReceiptId.New();

        shipment.ReceiveLine(
            receiptId,
            line.Id,
            WarehouseId,
            new CrateQuantity(40),
            Now.AddDays(2));

        Should.Throw<InvalidOperationException>(() =>
            shipment.ReceiveLine(
            receiptId,
            line.Id,
            WarehouseId,
                new CrateQuantity(20),
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
            ShipmentReceiptId.New(),
            line.Id,
            WarehouseId,
            new CrateQuantity(10),
            Now.AddDays(2));

        Should.Throw<InvalidOperationException>(() =>
            shipment.Cancel(Now.AddDays(3)));
    }

    [Fact]
    public void Create_Should_Initialize_Clearance_Tracks()
    {
        var shipment = CreateShipment();

        shipment.CustomsStatus.ShouldBe(
            CustomsClearanceStatus.NotStarted);

        shipment.PlantQuarantineStatus.ShouldBe(
            PlantQuarantineStatus.NotRequired);

        shipment.FoodSafetyStatus.ShouldBe(
            FoodSafetyStatus.NotRequired);
    }

    [Fact]
    public void RecordClearanceEvent_Should_Update_Customs_Status_And_History()
    {
        var shipment = CreateShipment();
        var occurredAt = Now.AddDays(1);

        var eventId = shipment.RecordClearanceEvent(
            ClearanceAuthority.Customs,
            nameof(CustomsClearanceStatus.BillOfEntryFiled),
            "BE-123",
            "Filed with customs",
            occurredAt);

        shipment.CustomsStatus.ShouldBe(
            CustomsClearanceStatus.BillOfEntryFiled);

        shipment.BillOfEntryNumber.ShouldBe("BE-123");
        shipment.BillOfEntryDate.ShouldBe(occurredAt);

        var clearanceEvent =
            shipment.ClearanceEvents.ShouldHaveSingleItem();

        clearanceEvent.Id.ShouldBe(eventId);
        clearanceEvent.Authority.ShouldBe(
            ClearanceAuthority.Customs);
    }

    [Fact]
    public void RecordClearanceEvent_Should_Update_FoodSafety_Status()
    {
        var shipment = CreateShipment();

        shipment.RecordClearanceEvent(
            ClearanceAuthority.FoodSafety,
            nameof(FoodSafetyStatus.LabTesting),
            null,
            null,
            Now.AddDays(1));

        shipment.FoodSafetyStatus.ShouldBe(
            FoodSafetyStatus.LabTesting);
    }

    [Fact]
    public void AddDocument_Should_Add_Document_Reference()
    {
        var shipment = CreateShipment();

        var documentId = shipment.AddDocument(
            ShipmentDocumentType.PhytosanitaryCertificate,
            "Phytosanitary certificate",
            "PHYTO-001",
            null,
            Now.AddDays(1));

        var document =
            shipment.Documents.ShouldHaveSingleItem();

        document.Id.ShouldBe(documentId);
        document.Type.ShouldBe(
            ShipmentDocumentType.PhytosanitaryCertificate);
        document.ReferenceNumber.ShouldBe("PHYTO-001");
    }

    [Fact]
    public void CreateWarehouseDelivery_Should_Require_Clearance()
    {
        var shipment = CreateArrivedShipment();
        var line = shipment.Lines.Single();

        Should.Throw<InvalidOperationException>(() =>
            shipment.CreateWarehouseDelivery(
                WarehouseId,
                "Fruit Logistics",
                "Driver One",
                "+27 82 000 0000",
                "CA 123-456",
                "Cape Town Port",
                "Cape Town Warehouse",
                "DLV-001",
                null,
                [(line.Id, 20)],
                Now.AddDays(2)));
    }

    [Fact]
    public void CreateWarehouseDelivery_Should_Prevent_OverAllocation()
    {
        var shipment = CreateClearedArrivedShipment();
        var line = shipment.Lines.Single();

        shipment.CreateWarehouseDelivery(
            WarehouseId,
            "Fruit Logistics",
            "Driver One",
            "+27 82 000 0000",
            "CA 123-456",
            "Cape Town Port",
            "Cape Town Warehouse",
            "DLV-001",
            null,
            [(line.Id, 70)],
            Now.AddDays(2));

        Should.Throw<InvalidOperationException>(() =>
            shipment.CreateWarehouseDelivery(
                Guid.NewGuid(),
                "Fruit Logistics",
                "Driver Two",
                "+27 82 111 1111",
                "CA 654-321",
                "Cape Town Port",
                "Johannesburg Warehouse",
                "DLV-002",
                null,
                [(line.Id, 40)],
                Now.AddDays(2)));
    }

    [Fact]
    public void ConfirmWarehouseDelivery_Should_Create_Receipt_Linked_To_Delivery()
    {
        var shipment = CreateClearedArrivedShipment();
        var line = shipment.Lines.Single();

        var deliveryId =
            shipment.CreateWarehouseDelivery(
                WarehouseId,
                "Fruit Logistics",
                "Driver One",
                "+27 82 000 0000",
                "CA 123-456",
                "Cape Town Port",
                "Cape Town Warehouse",
                "DLV-001",
                null,
                [(line.Id, 40)],
                Now.AddDays(2));

        shipment.DispatchWarehouseDelivery(
            deliveryId,
            Now.AddDays(2).AddHours(1));

        Should.Throw<InvalidOperationException>(() =>
            shipment.ConfirmWarehouseDelivery(deliveryId, Now.AddDays(2).AddHours(2)));

        shipment.MarkWarehouseDeliveryDelivered(deliveryId, Now.AddDays(2).AddHours(3));
        shipment.Deliveries.Single().Status.ShouldBe(ShipmentDeliveryStatus.DeliveredToWarehouse);
        line.ReceivedQuantity.Value.ShouldBe(0);
        shipment.Receipts.ShouldBeEmpty();

        var receipts =
            shipment.ConfirmWarehouseDelivery(
                deliveryId,
                Now.AddDays(2).AddHours(5));

        receipts.Count.ShouldBe(1);
        receipts.Single().DeliveryId.ShouldBe(deliveryId.Value);
        receipts.Single().WarehouseId.ShouldBe(WarehouseId);
        receipts.Single().Quantity.ShouldBe(40);

        shipment.Deliveries.Single().Status
            .ShouldBe(ShipmentDeliveryStatus.Received);
        line.ReceivedQuantity.Value.ShouldBe(40);
    }

    [Fact]
    public void PurchaseCost_CanBeEnteredAfterShipmentIsShipped()
    {
        var shipment = CreateShipment();
        var lineId = shipment.AddLine(Guid.NewGuid(), Guid.NewGuid(),
            new BatchCode("pending-cost"), new CrateQuantity(100), new PackSize(18m),
            null, Now);
        shipment.Lines.Single().UnitCost.ShouldBeNull();
        shipment.MarkShipped(Now.AddHours(1));
        shipment.SetLinePurchaseCost(lineId, new Money(250m, new Currency("ZAR")), Now.AddHours(2));
        shipment.Lines.Single().UnitCost!.Amount.ShouldBe(250m);
        shipment.Lines.Single().UnitCost!.Currency.Code.ShouldBe("ZAR");
        shipment.Status.ShouldBe(ShipmentStatus.Shipped);
    }

    [Fact]
    public void CountBasedPackaging_PreservesFruitCountAndPackageQuantity()
    {
        var shipment = CreateShipment();
        shipment.AddLine(Guid.NewGuid(), Guid.NewGuid(), new BatchCode("count-113"),
            new CrateQuantity(100), PackSize.ByCount(113), new Money(18.50m, new Currency("USD")), Now);

        var line = shipment.Lines.Single();
        line.PackSize.FruitCount.ShouldBe(113);
        line.PackSize.WeightKg.ShouldBeNull();
        line.ExpectedQuantity.Value.ShouldBe(100);
        line.UnitCost!.Amount.ShouldBe(18.50m);
    }

    [Fact]
    public void RegulatorySamples_ShouldReduceAllocatableQuantityWithoutChangingOriginalQuantity()
    {
        var shipment = CreateClearedArrivedShipment();
        var line = shipment.Lines.Single();
        var sampleId = Guid.NewGuid();
        shipment.RecordRegulatorySample(sampleId, line.Id, ClearanceAuthority.Customs, 6, "TEST-001", "Retained for testing", Now.AddDays(2));
        shipment.RecordRegulatorySample(sampleId, line.Id, ClearanceAuthority.Customs, 6, "TEST-001", "Retained for testing", Now.AddDays(2));
        shipment.RegulatorySamples.Count.ShouldBe(1);
        line.ExpectedQuantity.Value.ShouldBe(100);
        Should.Throw<InvalidOperationException>(() => shipment.CreateWarehouseDelivery(WarehouseId, "Fruit Logistics", "Driver", "123", null, "Port", "Warehouse", null, null, [(line.Id, 95)], Now.AddDays(2)));
        var deliveryId = shipment.CreateWarehouseDelivery(WarehouseId, "Fruit Logistics", "Driver", "123", null, "Port", "Warehouse", null, null, [(line.Id, 94)], Now.AddDays(2));
        shipment.DispatchWarehouseDelivery(deliveryId, Now.AddDays(2).AddHours(1));
        shipment.MarkWarehouseDeliveryDelivered(deliveryId, Now.AddDays(2).AddHours(2));
        shipment.ConfirmWarehouseDelivery(deliveryId, Now.AddDays(2).AddHours(3));
        line.ReceivedQuantity.Value.ShouldBe(94);
        shipment.Status.ShouldBe(ShipmentStatus.Received);
    }

    private static InboundShipment CreateShipment()
    {
        return InboundShipment.Create(
            TenantId,
            SupplierId,
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

    private static InboundShipment CreateClearedArrivedShipment()
    {
        var shipment = CreateArrivedShipment();

        shipment.RecordClearanceEvent(
            ClearanceAuthority.Customs,
            nameof(CustomsClearanceStatus.Cleared),
            "BOE-001",
            null,
            Now.AddDays(1).AddHours(1));

        return shipment;
    }
}
