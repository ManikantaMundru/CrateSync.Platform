using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class InboundShipment : AggregateRoot<InboundShipmentId>
{
    private readonly List<ShipmentLine> _lines = [];
    private readonly List<RegulatorySample> _regulatorySamples = [];
    private readonly List<ShipmentClearanceEvent> _clearanceEvents = [];
    private readonly List<ShipmentDocument> _documents = [];
    private readonly List<ShipmentReceipt> _receipts = [];
    private readonly List<ShipmentDelivery> _deliveries = [];

    private InboundShipment() { }

    private InboundShipment(
        InboundShipmentId id,
        Guid tenantId,
        Guid supplierId,
        TrackingNumber trackingNumber,
        CountryCode originCountry,
        DateTimeOffset expectedArrivalAtUtc,
        DateTimeOffset occurredOnUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("Supplier ID cannot be empty.", nameof(supplierId));

        TenantId = tenantId;
        SupplierId = supplierId;
        TrackingNumber = trackingNumber ?? throw new ArgumentNullException(nameof(trackingNumber));
        OriginCountry = originCountry ?? throw new ArgumentNullException(nameof(originCountry));
        ExpectedArrivalAtUtc = expectedArrivalAtUtc;
        Status = ShipmentStatus.Draft;
        CustomsStatus = CustomsClearanceStatus.NotStarted;
        PlantQuarantineStatus = PlantQuarantineStatus.NotRequired;
        FoodSafetyStatus = FoodSafetyStatus.NotRequired;
        CreatedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid SupplierId { get; private set; }

    public TrackingNumber TrackingNumber { get; private set; } = null!;

    public CountryCode OriginCountry { get; private set; } = null!;

    public DateTimeOffset ExpectedArrivalAtUtc { get; private set; }

    public DateTimeOffset? ShippedAtUtc { get; private set; }

    public DateTimeOffset? ArrivedAtUtc { get; private set; }

    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public ShipmentStatus Status { get; private set; }

    public string? ContainerNumber { get; private set; }

    public string? BillOfLadingNumber { get; private set; }

    public string? PortOfLoading { get; private set; }

    public string? PortOfDischarge { get; private set; }

    public string? VesselName { get; private set; }

    public string? VoyageNumber { get; private set; }

    public string? CustomsBrokerName { get; private set; }

    public string? BillOfEntryNumber { get; private set; }

    public DateTimeOffset? BillOfEntryDate { get; private set; }

    public DateTimeOffset? OutOfChargeAtUtc { get; private set; }

    public CustomsClearanceStatus CustomsStatus { get; private set; }

    public PlantQuarantineStatus PlantQuarantineStatus { get; private set; }

    public FoodSafetyStatus FoodSafetyStatus { get; private set; }

    public IReadOnlyCollection<RegulatorySample> RegulatorySamples => _regulatorySamples.AsReadOnly();

    public Guid RecordRegulatorySample(Guid sampleId, ShipmentLineId lineId, ClearanceAuthority authority,
        int quantity, string? referenceNumber, string remarks, DateTimeOffset occurredOnUtc)
    {
        if (sampleId == Guid.Empty || quantity <= 0 || !Enum.IsDefined(authority) ||
            string.IsNullOrWhiteSpace(remarks) || remarks.Length > 500 || referenceNumber?.Length > 100)
            throw new InvalidOperationException("Invalid sample quantity, authority, reference or remarks.");
        var existing = _regulatorySamples.SingleOrDefault(x => x.Id == sampleId);
        if (existing is not null)
        {
            if (existing.ShipmentLineId != lineId.Value || existing.Authority != authority || existing.Quantity != quantity ||
                existing.ReferenceNumber != referenceNumber?.Trim() || existing.Remarks != remarks.Trim())
                throw new InvalidOperationException("Sample ID already used for a different deduction.");
            return existing.Id;
        }
        if (Status != ShipmentStatus.Arrived || _deliveries.Count != 0 || _receipts.Count != 0)
            throw new InvalidOperationException("Record retained samples after arrival and before planning any warehouse delivery.");
        var line = _lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Shipment line was not found.");
        if (quantity > line.ExpectedQuantity.Value - SampleQuantity(lineId))
            throw new InvalidOperationException("Sample quantity exceeds remaining shipment packages.");
        _regulatorySamples.Add(new RegulatorySample(sampleId, lineId.Value, authority, quantity, referenceNumber, remarks, occurredOnUtc));
        UpdatedAtUtc = occurredOnUtc;
        UpdateReceiptCompletion(occurredOnUtc);
        return sampleId;
    }

    private int SampleQuantity(ShipmentLineId lineId) =>
        _regulatorySamples.Where(x => x.ShipmentLineId == lineId.Value).Sum(x => x.Quantity);

    private void UpdateReceiptCompletion(DateTimeOffset occurredOnUtc)
    {
        if (Status == ShipmentStatus.Received || _lines.Count == 0 ||
            !_lines.All(x => x.ReceivedQuantity.Value + SampleQuantity(x.Id) == x.ExpectedQuantity.Value))
            return;
        Status = ShipmentStatus.Received;
        ReceivedAtUtc = occurredOnUtc;
        RaiseDomainEvent(new ShipmentReceivedDomainEvent(Id, TenantId, SupplierId, occurredOnUtc));
    }

    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<ShipmentClearanceEvent> ClearanceEvents =>
        _clearanceEvents.AsReadOnly();

    public IReadOnlyCollection<ShipmentDocument> Documents =>
        _documents.AsReadOnly();

    public IReadOnlyCollection<ShipmentReceipt> Receipts =>
        _receipts.AsReadOnly();

    public IReadOnlyCollection<ShipmentDelivery> Deliveries =>
        _deliveries.AsReadOnly();

    public bool IsClearedForWarehouseDispatch =>
        (CustomsStatus is CustomsClearanceStatus.Cleared or CustomsClearanceStatus.OutOfCharge) &&
        (PlantQuarantineStatus is PlantQuarantineStatus.NotRequired or PlantQuarantineStatus.Cleared) &&
        (FoodSafetyStatus is FoodSafetyStatus.NotRequired or FoodSafetyStatus.NocIssued);

    public static InboundShipment Create(
        Guid tenantId,
        Guid supplierId,
        TrackingNumber trackingNumber,
        CountryCode originCountry,
        DateTimeOffset expectedArrivalAtUtc,
        DateTimeOffset occurredOnUtc)
    {
        var shipment = new InboundShipment(
            InboundShipmentId.New(),
            tenantId,
            supplierId,
            trackingNumber,
            originCountry,
            expectedArrivalAtUtc,
            occurredOnUtc);

        shipment.RaiseDomainEvent(
            new ShipmentCreatedDomainEvent(
                shipment.Id,
                shipment.TenantId,
                shipment.SupplierId,
                occurredOnUtc));

        return shipment;
    }

    public void UpdateShippingAndCustomsReferences(
        string? containerNumber,
        string? billOfLadingNumber,
        string? portOfLoading,
        string? portOfDischarge,
        string? vesselName,
        string? voyageNumber,
        string? customsBrokerName,
        string? billOfEntryNumber,
        DateTimeOffset? billOfEntryDate,
        DateTimeOffset? outOfChargeAtUtc,
        DateTimeOffset occurredOnUtc)
    {
        ContainerNumber = NormalizeOptional(containerNumber, 50, nameof(containerNumber));
        BillOfLadingNumber = NormalizeOptional(billOfLadingNumber, 100, nameof(billOfLadingNumber));
        PortOfLoading = NormalizeOptional(portOfLoading, 150, nameof(portOfLoading));
        PortOfDischarge = NormalizeOptional(portOfDischarge, 150, nameof(portOfDischarge));
        VesselName = NormalizeOptional(vesselName, 150, nameof(vesselName));
        VoyageNumber = NormalizeOptional(voyageNumber, 100, nameof(voyageNumber));
        CustomsBrokerName = NormalizeOptional(customsBrokerName, 150, nameof(customsBrokerName));
        BillOfEntryNumber = NormalizeOptional(billOfEntryNumber, 100, nameof(billOfEntryNumber));
        BillOfEntryDate = billOfEntryDate;
        OutOfChargeAtUtc = outOfChargeAtUtc;
        UpdatedAtUtc = occurredOnUtc;
    }

    public ShipmentClearanceEventId RecordClearanceEvent(
        ClearanceAuthority authority,
        string status,
        string? referenceNumber,
        string? remarks,
        DateTimeOffset occurredOnUtc)
    {
        ApplyClearanceStatus(authority, status, occurredOnUtc);

        if (
            authority == ClearanceAuthority.Customs &&
            Enum.TryParse<CustomsClearanceStatus>(
                status,
                true,
                out var customsStatus) &&
            customsStatus == CustomsClearanceStatus.BillOfEntryFiled)
        {
            if (!string.IsNullOrWhiteSpace(referenceNumber))
            {
                BillOfEntryNumber =
                    NormalizeOptional(
                        referenceNumber,
                        100,
                        nameof(referenceNumber));
            }

            BillOfEntryDate ??= occurredOnUtc;
        }

        var clearanceEvent = new ShipmentClearanceEvent(
            ShipmentClearanceEventId.New(),
            authority,
            status,
            referenceNumber,
            remarks,
            occurredOnUtc);

        _clearanceEvents.Add(clearanceEvent);
        UpdatedAtUtc = occurredOnUtc;

        return clearanceEvent.Id;
    }

    public ShipmentDocumentId AddDocument(
        ShipmentDocumentType type,
        string documentName,
        string? referenceNumber,
        string? notes,
        DateTimeOffset occurredOnUtc)
    {
        var document = new ShipmentDocument(
            ShipmentDocumentId.New(),
            type,
            documentName,
            referenceNumber,
            notes,
            occurredOnUtc);

        _documents.Add(document);
        UpdatedAtUtc = occurredOnUtc;

        return document.Id;
    }

    public ShipmentLineId AddLine(
        Guid productId,
        Guid productVarietyId,
        BatchCode batchCode,
        CrateQuantity expectedQuantity,
        PackSize packSize,
        Money? unitCost,
        DateTimeOffset occurredOnUtc)
    {
        EnsureDraft();

        if (_lines.Any(x => x.BatchCode == batchCode))
            throw new InvalidOperationException($"Batch code '{batchCode}' already exists on this shipment.");

        var line = new ShipmentLine(
            ShipmentLineId.New(),
            productId,
            productVarietyId,
            batchCode,
            expectedQuantity,
            packSize,
            unitCost);

        _lines.Add(line);
        UpdatedAtUtc = occurredOnUtc;

        return line.Id;
    }

    public void SetLinePurchaseCost(ShipmentLineId lineId, Money cost, DateTimeOffset occurredOnUtc)
    {
        if (Status == ShipmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot update costs on a cancelled shipment.");
        var line = _lines.SingleOrDefault(x => x.Id == lineId)
            ?? throw new InvalidOperationException("Shipment line was not found.");
        line.SetPurchaseCost(cost);
        UpdatedAtUtc = occurredOnUtc;
    }

    public void MarkShipped(DateTimeOffset occurredOnUtc)
    {
        if (Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Only draft shipments can be marked as shipped.");

        if (_lines.Count == 0)
            throw new InvalidOperationException("Shipment must contain at least one line before it can be shipped.");

        Status = ShipmentStatus.Shipped;
        ShippedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(new ShipmentShippedDomainEvent(Id, TenantId, occurredOnUtc));
    }

    public void MarkArrived(DateTimeOffset occurredOnUtc)
    {
        if (Status != ShipmentStatus.Shipped)
            throw new InvalidOperationException("Only shipped shipments can be marked as arrived.");

        Status = ShipmentStatus.Arrived;
        ArrivedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(new ShipmentArrivedDomainEvent(Id, TenantId, occurredOnUtc));
    }

    public ShipmentReceiptId ReceiveLine(
        ShipmentReceiptId receiptId,
        ShipmentLineId shipmentLineId,
        Guid warehouseId,
        CrateQuantity quantity,
        DateTimeOffset occurredOnUtc,
        ShipmentDeliveryId? deliveryId = null)
    {
        var existingReceipt =
            _receipts.SingleOrDefault(
                x => x.Id == receiptId);

        if (existingReceipt is not null)
        {
            if (
                existingReceipt.ShipmentLineId != shipmentLineId ||
                existingReceipt.WarehouseId != warehouseId ||
                existingReceipt.DeliveryId != deliveryId?.Value ||
                existingReceipt.Quantity != quantity.Value)
            {
                throw new InvalidOperationException(
                    "Receipt ID has already been used for a different receipt.");
            }

            return existingReceipt.Id;
        }

        if (Status is not ShipmentStatus.Arrived and
            not ShipmentStatus.Receiving and
            not ShipmentStatus.PartiallyReceived)
        {
            throw new InvalidOperationException("Shipment must have arrived before stock can be received.");
        }

        var line = _lines.SingleOrDefault(x => x.Id == shipmentLineId);

        if (line is null)
            throw new InvalidOperationException($"Shipment line '{shipmentLineId.Value}' was not found.");

        if (Status == ShipmentStatus.Arrived)
        {
            Status = ShipmentStatus.Receiving;

            RaiseDomainEvent(
                new ShipmentReceivingStartedDomainEvent(
                    Id,
                    TenantId,
                    occurredOnUtc));
        }

        if (quantity.Value > line.ExpectedQuantity.Value - line.ReceivedQuantity.Value - SampleQuantity(line.Id))
            throw new InvalidOperationException("Receipt exceeds quantity net of regulatory samples.");
        line.Receive(quantity);

        var receipt = new ShipmentReceipt(
            receiptId,
            line.Id,
            warehouseId,
            deliveryId?.Value,
            line.ProductVarietyId,
            quantity.Value,
            occurredOnUtc);

        _receipts.Add(receipt);
        UpdatedAtUtc = occurredOnUtc;

        if (_lines.All(x => x.ReceivedQuantity.Value + SampleQuantity(x.Id) == x.ExpectedQuantity.Value))
        {
            Status = ShipmentStatus.Received;
            ReceivedAtUtc = occurredOnUtc;

            RaiseDomainEvent(
                new ShipmentReceivedDomainEvent(
                    Id,
                    TenantId,
                    SupplierId,
                    occurredOnUtc));

            return receipt.Id;
        }

        Status = ShipmentStatus.PartiallyReceived;

        RaiseDomainEvent(
            new ShipmentPartiallyReceivedDomainEvent(
                Id,
                TenantId,
                occurredOnUtc));

        return receipt.Id;
    }

    public ShipmentDeliveryId CreateWarehouseDelivery(
        Guid warehouseId,
        string transportCompany,
        string driverName,
        string driverPhone,
        string? vehicleNumber,
        string originLocation,
        string destinationLocation,
        string? transportReference,
        string? remarks,
        IReadOnlyCollection<(ShipmentLineId LineId, int Quantity)> allocations,
        DateTimeOffset occurredOnUtc)
    {
        if (Status is not ShipmentStatus.Arrived and
            not ShipmentStatus.Receiving and
            not ShipmentStatus.PartiallyReceived)
        {
            throw new InvalidOperationException(
                "Shipment must have arrived before warehouse delivery can be planned.");
        }

        if (!IsClearedForWarehouseDispatch)
        {
            throw new InvalidOperationException(
                "Shipment must complete required Customs, Plant Quarantine and Food Safety clearance before warehouse dispatch.");
        }

        if (allocations.Count == 0)
            throw new InvalidOperationException("Warehouse delivery must contain at least one allocation.");

        var delivery = new ShipmentDelivery(
            ShipmentDeliveryId.New(),
            warehouseId,
            transportCompany,
            driverName,
            driverPhone,
            vehicleNumber,
            originLocation,
            destinationLocation,
            transportReference,
            remarks,
            occurredOnUtc);

        foreach (var allocation in allocations)
        {
            var line = _lines.SingleOrDefault(x => x.Id == allocation.LineId);

            if (line is null)
                throw new InvalidOperationException(
                    $"Shipment line '{allocation.LineId.Value}' was not found.");

            var alreadyAllocated =
                _deliveries
                    .Where(x =>
                        x.Status is ShipmentDeliveryStatus.Planned or ShipmentDeliveryStatus.InTransit or ShipmentDeliveryStatus.DeliveredToWarehouse)
                    .SelectMany(x => x.Allocations)
                    .Where(x => x.ShipmentLineId == allocation.LineId)
                    .Sum(x => x.Quantity);

            var remaining =
                line.ExpectedQuantity.Value - SampleQuantity(line.Id) -
                line.ReceivedQuantity.Value -
                alreadyAllocated - _deliveries.SelectMany(x => x.Allocations)
                    .Where(x => x.ShipmentLineId == allocation.LineId)
                    .Sum(x => (x.DamagedQuantity ?? 0) + (x.MissingQuantity ?? 0));

            if (allocation.Quantity <= 0 ||
                allocation.Quantity > remaining)
            {
                throw new InvalidOperationException(
                    $"Shipment line '{allocation.LineId.Value}' has only {remaining} crates available for allocation.");
            }

            delivery.AddAllocation(
                allocation.LineId,
                allocation.Quantity);
        }

        _deliveries.Add(delivery);
        UpdatedAtUtc = occurredOnUtc;

        return delivery.Id;
    }

    public void DispatchWarehouseDelivery(
        ShipmentDeliveryId deliveryId,
        DateTimeOffset occurredOnUtc)
    {
        if (!IsClearedForWarehouseDispatch)
            throw new InvalidOperationException("Shipment is not cleared for warehouse dispatch.");

        var delivery =
            _deliveries.SingleOrDefault(x => x.Id == deliveryId)
            ?? throw new InvalidOperationException("Warehouse delivery was not found.");

        delivery.MarkDispatched(occurredOnUtc);
        UpdatedAtUtc = occurredOnUtc;
    }

    public void MarkWarehouseDeliveryDelivered(
        ShipmentDeliveryId deliveryId,
        DateTimeOffset occurredOnUtc)
    {
        if (Status == ShipmentStatus.Cancelled)
            throw new InvalidOperationException("A cancelled shipment cannot be delivered.");
        var delivery = _deliveries.SingleOrDefault(x => x.Id == deliveryId)
            ?? throw new InvalidOperationException("Warehouse delivery was not found.");
        if (delivery.Status == ShipmentDeliveryStatus.DeliveredToWarehouse)
            return;
        delivery.MarkDelivered();
        UpdatedAtUtc = occurredOnUtc;
    }

    public IReadOnlyCollection<ShipmentReceipt> ConfirmWarehouseDelivery(
        ShipmentDeliveryId deliveryId,
        DateTimeOffset occurredOnUtc,
        IReadOnlyCollection<(Guid AllocationId, int Accepted, int Damaged, int Missing, string? Remarks)>? inspections = null)
    {
        var delivery = _deliveries.SingleOrDefault(x => x.Id == deliveryId)
            ?? throw new InvalidOperationException("Warehouse delivery was not found.");

        if (delivery.Status == ShipmentDeliveryStatus.Received)
        {
            if (inspections is not null && (inspections.Count != delivery.Allocations.Count ||
                inspections.Select(x => x.AllocationId).Distinct().Count() != inspections.Count ||
                inspections.Any(x => !delivery.Allocations.Any(a => a.Id.Value == x.AllocationId &&
                    (a.AcceptedQuantity ?? a.Quantity) == x.Accepted &&
                    (a.DamagedQuantity ?? 0) == x.Damaged && (a.MissingQuantity ?? 0) == x.Missing &&
                    (a.ReceivingRemarks ?? "") == (x.Remarks?.Trim() ?? "")))))
                throw new InvalidOperationException("Delivery was already received with different inspection quantities.");
            return _receipts.Where(x => x.DeliveryId == deliveryId.Value).ToArray();
        }

        if (delivery.Status != ShipmentDeliveryStatus.DeliveredToWarehouse)
            throw new InvalidOperationException("Only delivered transports can be received.");
        if (Status is not ShipmentStatus.Arrived and not ShipmentStatus.Receiving and not ShipmentStatus.PartiallyReceived)
            throw new InvalidOperationException("Shipment cannot be received in its current status.");

        var entries = inspections ?? delivery.Allocations.Select(a =>
            (a.Id.Value, a.Quantity, 0, 0, (string?)null)).ToArray();
        if (entries.Count != delivery.Allocations.Count || entries.Select(x => x.AllocationId).Distinct().Count() != entries.Count)
            throw new InvalidOperationException("Provide exactly one inspection for every allocation.");

        // Validate the complete inspection before mutating the aggregate.
        foreach (var entry in entries)
        {
            var allocation = delivery.Allocations.SingleOrDefault(a => a.Id.Value == entry.AllocationId)
                ?? throw new InvalidOperationException("Unknown delivery allocation.");
            if (entry.Accepted < 0 || entry.Damaged < 0 || entry.Missing < 0 ||
                (long)entry.Accepted + entry.Damaged + entry.Missing != allocation.Quantity ||
                entry.Remarks?.Length > 500 ||
                ((entry.Damaged > 0 || entry.Missing > 0) && string.IsNullOrWhiteSpace(entry.Remarks)))
                throw new InvalidOperationException("Invalid inspection quantities or missing discrepancy remarks.");
            var line = _lines.Single(x => x.Id == allocation.ShipmentLineId);
            if (entry.Accepted > line.ExpectedQuantity.Value - line.ReceivedQuantity.Value)
                throw new InvalidOperationException("Accepted quantity exceeds remaining shipment quantity.");
        }

        var receipts = new List<ShipmentReceipt>();
        foreach (var entry in entries)
        {
            var allocation = delivery.Allocations.Single(a => a.Id.Value == entry.AllocationId);
            allocation.RecordInspection(entry.Accepted, entry.Damaged, entry.Missing, entry.Remarks);
            if (entry.Accepted == 0) continue;
            var receiptId = ShipmentReceiptId.New();
            ReceiveLine(receiptId, allocation.ShipmentLineId, delivery.WarehouseId,
                new CrateQuantity(entry.Accepted), occurredOnUtc, delivery.Id);
            receipts.Add(_receipts.Single(x => x.Id == receiptId));
        }

        delivery.MarkReceived(occurredOnUtc);
        UpdatedAtUtc = occurredOnUtc;
        // A shortage is accounted for, not available for another transport.
        // Keep the shipment partially received when accepted stock is below expected.
        if (Status == ShipmentStatus.Arrived)
            Status = ShipmentStatus.PartiallyReceived;
        return receipts;
    }

    public void Cancel(DateTimeOffset occurredOnUtc)
    {
        if (Status is ShipmentStatus.Received or ShipmentStatus.Cancelled)
            throw new InvalidOperationException($"Shipment in status '{Status}' cannot be cancelled.");

        if (Status is ShipmentStatus.Receiving or ShipmentStatus.PartiallyReceived)
            throw new InvalidOperationException("A shipment cannot be cancelled after receiving has started.");

        Status = ShipmentStatus.Cancelled;
        CancelledAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;

        RaiseDomainEvent(new ShipmentCancelledDomainEvent(Id, TenantId, occurredOnUtc));
    }

    private void ApplyClearanceStatus(
        ClearanceAuthority authority,
        string status,
        DateTimeOffset occurredOnUtc)
    {
        switch (authority)
        {
            case ClearanceAuthority.Customs:
                if (!Enum.TryParse<CustomsClearanceStatus>(
                        status,
                        true,
                        out var customsStatus))
                {
                    throw new ArgumentException(
                        $"'{status}' is not a valid customs clearance status.",
                        nameof(status));
                }

                CustomsStatus = customsStatus;

                if (customsStatus == CustomsClearanceStatus.OutOfCharge)
                    OutOfChargeAtUtc ??= occurredOnUtc;

                break;

            case ClearanceAuthority.PlantQuarantine:
                if (!Enum.TryParse<PlantQuarantineStatus>(
                        status,
                        true,
                        out var plantQuarantineStatus))
                {
                    throw new ArgumentException(
                        $"'{status}' is not a valid plant quarantine status.",
                        nameof(status));
                }

                PlantQuarantineStatus = plantQuarantineStatus;
                break;

            case ClearanceAuthority.FoodSafety:
                if (!Enum.TryParse<FoodSafetyStatus>(
                        status,
                        true,
                        out var foodSafetyStatus))
                {
                    throw new ArgumentException(
                        $"'{status}' is not a valid food safety status.",
                        nameof(status));
                }

                FoodSafetyStatus = foodSafetyStatus;
                break;

            case ClearanceAuthority.Port:
            case ClearanceAuthority.Other:
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(authority),
                    authority,
                    null);
        }
    }

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);

        return trimmed;
    }

    private void EnsureDraft()
    {
        if (Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Shipment lines can only be changed while the shipment is in Draft status.");
    }
}
