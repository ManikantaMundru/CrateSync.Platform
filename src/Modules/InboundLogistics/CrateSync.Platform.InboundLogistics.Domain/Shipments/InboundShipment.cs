using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments;

public sealed class InboundShipment : AggregateRoot<InboundShipmentId>
{
    private readonly List<ShipmentLine> _lines = [];

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

    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();

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

    private void EnsureDraft()
    {
        if (Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Shipment lines can only be changed while the shipment is in Draft status.");
    }
}
