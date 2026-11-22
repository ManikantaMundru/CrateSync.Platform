using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class InboundShipment : AggregateRoot<InboundShipmentId>
{
    private readonly List<ShipmentLine> _lines = [];
    private readonly List<ShipmentClearanceEvent> _clearanceEvents = [];
    private readonly List<ShipmentDocument> _documents = [];

    private InboundShipment() { }

    private InboundShipment(
        InboundShipmentId id,
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        TrackingNumber trackingNumber,
        CountryCode originCountry,
        DateTimeOffset expectedArrivalAtUtc,
        DateTimeOffset occurredOnUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("Supplier ID cannot be empty.", nameof(supplierId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        TrackingNumber = trackingNumber ?? throw new ArgumentNullException(nameof(trackingNumber));
        OriginCountry = originCountry ?? throw new ArgumentNullException(nameof(originCountry));
        ExpectedArrivalAtUtc = expectedArrivalAtUtc;
        Status = ShipmentStatus.Draft;
        CustomsStatus = CustomsClearanceStatus.NotStarted;
        PlantQuarantineStatus = PlantQuarantineStatus.Pending;
        FoodSafetyStatus = FoodSafetyStatus.Pending;
        CreatedAtUtc = occurredOnUtc;
        UpdatedAtUtc = occurredOnUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid SupplierId { get; private set; }

    public Guid WarehouseId { get; private set; }

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

    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();

    public IReadOnlyCollection<ShipmentClearanceEvent> ClearanceEvents =>
        _clearanceEvents.AsReadOnly();

    public IReadOnlyCollection<ShipmentDocument> Documents =>
        _documents.AsReadOnly();

    public static InboundShipment Create(
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        TrackingNumber trackingNumber,
        CountryCode originCountry,
        DateTimeOffset expectedArrivalAtUtc,
        DateTimeOffset occurredOnUtc)
    {
        var shipment = new InboundShipment(
            InboundShipmentId.New(),
            tenantId,
            supplierId,
            warehouseId,
            trackingNumber,
            originCountry,
            expectedArrivalAtUtc,
            occurredOnUtc);

        shipment.RaiseDomainEvent(
            new ShipmentCreatedDomainEvent(
                shipment.Id,
                shipment.TenantId,
                shipment.SupplierId,
                shipment.WarehouseId,
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
        ApplyClearanceStatus(authority, status);

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
        Money unitCost,
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

        RaiseDomainEvent(new ShipmentArrivedDomainEvent(Id, TenantId, WarehouseId, occurredOnUtc));
    }

    public void ReceiveLine(
        ShipmentLineId shipmentLineId,
        CrateQuantity quantity,
        DateTimeOffset occurredOnUtc)
    {
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

        line.Receive(quantity);
        UpdatedAtUtc = occurredOnUtc;

        if (_lines.All(x => x.IsFullyReceived))
        {
            Status = ShipmentStatus.Received;
            ReceivedAtUtc = occurredOnUtc;

            RaiseDomainEvent(
                new ShipmentReceivedDomainEvent(
                    Id,
                    TenantId,
                    SupplierId,
                    WarehouseId,
                    occurredOnUtc));

            return;
        }

        Status = ShipmentStatus.PartiallyReceived;

        RaiseDomainEvent(
            new ShipmentPartiallyReceivedDomainEvent(
                Id,
                TenantId,
                occurredOnUtc));
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
        string status)
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
                    OutOfChargeAtUtc ??= DateTimeOffset.UtcNow;

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
