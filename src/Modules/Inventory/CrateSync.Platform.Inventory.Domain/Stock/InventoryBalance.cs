using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Inventory.Domain.Exceptions;
using CrateSync.Platform.Inventory.Domain.Stock.Events;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class InventoryBalance : AggregateRoot<InventoryBalanceId>
{
    private readonly List<StockMovement> _movements = [];

    private InventoryBalance() { }

    private InventoryBalance(
        InventoryBalanceId id,
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));

        if (productVarietyId == Guid.Empty)
            throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        ProductVarietyId = productVarietyId;

        OnHand = StockQuantity.Zero;
        Reserved = StockQuantity.Zero;

        CreatedAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
    }

    public Guid TenantId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid ProductVarietyId { get; private set; }

    public StockQuantity OnHand { get; private set; }

    public StockQuantity Reserved { get; private set; }

    public int Available => OnHand.Value - Reserved.Value;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<StockMovement> Movements =>
        _movements.AsReadOnly();

    public static InventoryBalance Create(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        DateTimeOffset occurredAtUtc)
    {
        return new InventoryBalance(
            InventoryBalanceId.New(),
            tenantId,
            warehouseId,
            productVarietyId,
            occurredAtUtc);
    }

    public void Receive(
        CrateQuantity quantity,
        StockReference reference,
        DateTimeOffset occurredAtUtc)
    {
        var newOnHand = OnHand.Value + quantity.Value;

        OnHand = new StockQuantity(newOnHand);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Received,
            quantity.Value,
            0,
            reference,
            null,
            occurredAtUtc);

        RaiseDomainEvent(
            new StockReceivedDomainEvent(
                Id,
                TenantId,
                WarehouseId,
                ProductVarietyId,
                quantity.Value,
                reference.Id,
                occurredAtUtc));
    }

    public void Reserve(
        CrateQuantity quantity,
        StockReference reference,
        DateTimeOffset occurredAtUtc)
    {
        if (quantity.Value > Available)
            throw new InsufficientStockException(
                Available,
                quantity.Value);

        Reserved = new StockQuantity(
            Reserved.Value + quantity.Value);

        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Reserved,
            0,
            quantity.Value,
            reference,
            null,
            occurredAtUtc);

        RaiseDomainEvent(
            new StockReservedDomainEvent(
                Id,
                TenantId,
                WarehouseId,
                ProductVarietyId,
                quantity.Value,
                reference.Id,
                occurredAtUtc));
    }

    public void ReleaseReservation(
        CrateQuantity quantity,
        StockReference reference,
        DateTimeOffset occurredAtUtc)
    {
        if (quantity.Value > Reserved.Value)
            throw new InsufficientReservedStockException(
                Reserved.Value,
                quantity.Value);

        Reserved = new StockQuantity(
            Reserved.Value - quantity.Value);

        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.ReservationReleased,
            0,
            -quantity.Value,
            reference,
            null,
            occurredAtUtc);

        RaiseDomainEvent(
            new StockReservationReleasedDomainEvent(
                Id,
                TenantId,
                WarehouseId,
                ProductVarietyId,
                quantity.Value,
                reference.Id,
                occurredAtUtc));
    }

    public void Dispatch(
        CrateQuantity quantity,
        StockReference reference,
        DateTimeOffset occurredAtUtc)
    {
        if (quantity.Value > Reserved.Value)
            throw new InsufficientReservedStockException(
                Reserved.Value,
                quantity.Value);

        if (quantity.Value > OnHand.Value)
            throw new InsufficientStockException(
                OnHand.Value,
                quantity.Value);

        OnHand = new StockQuantity(
            OnHand.Value - quantity.Value);

        Reserved = new StockQuantity(
            Reserved.Value - quantity.Value);

        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Dispatched,
            -quantity.Value,
            -quantity.Value,
            reference,
            null,
            occurredAtUtc);

        RaiseDomainEvent(
            new StockDispatchedDomainEvent(
                Id,
                TenantId,
                WarehouseId,
                ProductVarietyId,
                quantity.Value,
                reference.Id,
                occurredAtUtc));
    }

    public void Adjust(
        int quantityChange,
        AdjustmentReason reason,
        StockReference reference,
        DateTimeOffset occurredAtUtc)
    {
        if (quantityChange == 0)
            throw new ArgumentException(
                "Adjustment quantity cannot be zero.",
                nameof(quantityChange));

        var newOnHand = OnHand.Value + quantityChange;

        if (newOnHand < 0)
            throw new InsufficientStockException(
                OnHand.Value,
                Math.Abs(quantityChange));

        if (newOnHand < Reserved.Value)
            throw new InvalidOperationException(
                "Adjustment would reduce on-hand stock below reserved stock.");

        OnHand = new StockQuantity(newOnHand);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Adjusted,
            quantityChange,
            0,
            reference,
            reason.Value,
            occurredAtUtc);

        RaiseDomainEvent(
            new StockAdjustedDomainEvent(
                Id,
                TenantId,
                WarehouseId,
                ProductVarietyId,
                quantityChange,
                reason.Value,
                occurredAtUtc));
    }

    private void AddMovement(
        StockMovementType type,
        int onHandChange,
        int reservedChange,
        StockReference reference,
        string? reason,
        DateTimeOffset occurredAtUtc)
    {
        var movement = new StockMovement(
            StockMovementId.New(),
            type,
            onHandChange,
            reservedChange,
            OnHand.Value,
            Reserved.Value,
            reference,
            reason,
            occurredAtUtc);

        _movements.Add(movement);
    }
}
