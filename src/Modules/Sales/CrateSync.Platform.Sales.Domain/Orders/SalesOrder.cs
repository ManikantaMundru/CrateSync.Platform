using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Sales.Domain.Orders.Events;
using CrateSync.Platform.Sales.Domain.ValueObjects;

namespace CrateSync.Platform.Sales.Domain.Orders;

public sealed class SalesOrder : AggregateRoot<SalesOrderId>
{
    private readonly List<SalesOrderLine> _lines = [];

    private SalesOrder()
    {
        OrderNumber = null!;
        DeliveryAddress = null!;
    }

    private SalesOrder(
        SalesOrderId id,
        Guid tenantId,
        Guid customerId,
        Guid warehouseId,
        OrderNumber orderNumber,
        DeliveryAddress deliveryAddress,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        WarehouseId = warehouseId;
        OrderNumber = orderNumber;
        DeliveryAddress = deliveryAddress;
        Status = SalesOrderStatus.Draft;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        RaiseDomainEvent(new SalesOrderCreatedDomainEvent(
            Id, TenantId, CustomerId, OrderNumber.Value, createdAtUtc));
    }

    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public OrderNumber OrderNumber { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public string? ReservationFailureReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public DateTimeOffset? AllocatedAtUtc { get; private set; }
    public DateTimeOffset? DispatchedAtUtc { get; private set; }
    public DateTimeOffset? FulfilledAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(x => x.LineTotal);

    public static SalesOrder Create(
        Guid tenantId,
        Guid customerId,
        Guid warehouseId,
        OrderNumber orderNumber,
        DeliveryAddress deliveryAddress,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required.", nameof(tenantId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("Warehouse ID is required.", nameof(warehouseId));

        return new SalesOrder(
            SalesOrderId.New(),
            tenantId,
            customerId,
            warehouseId,
            orderNumber,
            deliveryAddress,
            createdAtUtc);
    }

    public SalesOrderLine AddLine(
        Guid productVarietyId,
        CrateQuantity quantity,
        PackSize packSize,
        Money unitPrice,
        DateTimeOffset occurredAtUtc,
        string originCountry = "ZZ", Guid batchId = default, string? batchCode = null, int? fruitsPerPack = null)
    {
        EnsureDraft();
        if (batchId != Guid.Empty && (string.IsNullOrWhiteSpace(batchCode) || batchCode.Length > 100 || fruitsPerPack is <= 0)) throw new InvalidOperationException("Invalid batch packaging.");

        if (string.IsNullOrWhiteSpace(originCountry))
            throw new ArgumentException(
                "Origin country is required.",
                nameof(originCountry));

        var normalizedOrigin =
            originCountry.Trim().ToUpperInvariant();

        if (_lines.Any(x =>
                x.ProductVarietyId == productVarietyId &&
                x.OriginCountry == normalizedOrigin && x.BatchId == batchId))
        {
            throw new InvalidOperationException(
                "Product variety and origin already exist on this order.");
        }

        EnsureCurrency(unitPrice);

        var line = new SalesOrderLine(
            SalesOrderLineId.New(),
            productVarietyId,
            quantity,
            packSize,
            unitPrice,
            normalizedOrigin, batchId, batchCode, fruitsPerPack);

        _lines.Add(line);
        UpdatedAtUtc = occurredAtUtc;
        return line;
    }

    public void UpdateLine(
        SalesOrderLineId lineId,
        CrateQuantity quantity,
        PackSize packSize,
        Money unitPrice,
        DateTimeOffset occurredAtUtc)
    {
        EnsureDraft();
        EnsureCurrency(unitPrice);
        GetLine(lineId).Update(quantity, packSize, unitPrice);
        UpdatedAtUtc = occurredAtUtc;
    }

    public void RemoveLine(SalesOrderLineId lineId, DateTimeOffset occurredAtUtc)
    {
        EnsureDraft();
        _lines.Remove(GetLine(lineId));
        UpdatedAtUtc = occurredAtUtc;
    }

    public void Confirm(DateTimeOffset occurredAtUtc)
    {
        if (Status == SalesOrderStatus.Confirmed) return;
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only draft sales orders can be confirmed.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Sales order must contain at least one line.");

        Status = SalesOrderStatus.Confirmed;
        ConfirmedAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
        ReservationFailureReason = null;

        RaiseDomainEvent(new SalesOrderConfirmedDomainEvent(Id, TenantId, occurredAtUtc));
    }

    public void Allocate(DateTimeOffset occurredAtUtc)
    {
        if (Status == SalesOrderStatus.Allocated) return;
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed sales orders can be allocated.");

        Status = SalesOrderStatus.Allocated;
        AllocatedAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
        ReservationFailureReason = null;

        RaiseDomainEvent(new SalesOrderAllocatedDomainEvent(Id, TenantId, occurredAtUtc));
    }

    public void RecordReservationFailure(string reason, DateTimeOffset occurredAtUtc)
    {
        if (Status != SalesOrderStatus.Confirmed) return;
        ReservationFailureReason = reason;
        UpdatedAtUtc = occurredAtUtc;
    }

    public void RequestDispatch(DateTimeOffset occurredAtUtc)
    {
        if (Status != SalesOrderStatus.Allocated)
            throw new InvalidOperationException("Only allocated sales orders can request dispatch.");

        RaiseDomainEvent(new SalesOrderDispatchRequestedDomainEvent(Id, TenantId, occurredAtUtc));
    }

    public void Dispatch(DateTimeOffset occurredAtUtc)
    {
        if (Status is SalesOrderStatus.Dispatched or SalesOrderStatus.Fulfilled) return;
        if (Status != SalesOrderStatus.Allocated)
            throw new InvalidOperationException("Only allocated sales orders can be marked dispatched.");

        Status = SalesOrderStatus.Dispatched;
        DispatchedAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new SalesOrderDispatchedDomainEvent(Id, TenantId, occurredAtUtc));
    }

    public void Fulfil(DateTimeOffset occurredAtUtc)
    {
        if (Status == SalesOrderStatus.Fulfilled) return;
        if (Status != SalesOrderStatus.Dispatched)
            throw new InvalidOperationException("Only dispatched sales orders can be fulfilled.");

        Status = SalesOrderStatus.Fulfilled;
        FulfilledAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new SalesOrderFulfilledDomainEvent(Id, TenantId, occurredAtUtc));
    }

    public void Cancel(DateTimeOffset occurredAtUtc)
    {
        if (Status == SalesOrderStatus.Cancelled) return;
        if (Status is SalesOrderStatus.Dispatched or SalesOrderStatus.Fulfilled)
            throw new InvalidOperationException("A dispatched or fulfilled order cannot be cancelled.");

        Status = SalesOrderStatus.Cancelled;
        CancelledAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;

        RaiseDomainEvent(new SalesOrderCancelledDomainEvent(Id, TenantId, occurredAtUtc));
    }

    private SalesOrderLine GetLine(SalesOrderLineId lineId) =>
        _lines.SingleOrDefault(x => x.Id == lineId)
        ?? throw new InvalidOperationException("Sales order line was not found.");

    private void EnsureDraft()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only draft sales orders can be edited.");
    }

    private void EnsureCurrency(Money unitPrice)
    {
        var currency = _lines.Select(x => x.UnitPrice.Currency.Code).FirstOrDefault();

        if (currency is not null &&
            !string.Equals(currency, unitPrice.Currency.Code, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("All order lines must use the same currency.");
        }
    }
}
