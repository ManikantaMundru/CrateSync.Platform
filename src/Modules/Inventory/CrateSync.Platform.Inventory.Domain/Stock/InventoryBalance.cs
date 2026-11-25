using CrateSync.Platform.BuildingBlocks.Domain;
using CrateSync.Platform.Inventory.Domain.Exceptions;
using CrateSync.Platform.Inventory.Domain.Stock.Events;
using CrateSync.Platform.Inventory.Domain.ValueObjects;

namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class InventoryBalance : AggregateRoot<InventoryBalanceId>
{
    private readonly List<SellingPriceChange> _sellingPriceHistory = [];
    private readonly List<StockMovement> _movements = [];
    private readonly List<StockReservation> _reservations = [];

    private InventoryBalance() { }

    private InventoryBalance(
        InventoryBalanceId id,
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        WarehouseId = warehouseId;
        ProductVarietyId = productVarietyId;
        OriginCountry = NormalizeOriginCountry(originCountry);
        OnHand = StockQuantity.Zero;
        Reserved = StockQuantity.Zero;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductVarietyId { get; private set; }
    public string OriginCountry { get; private set; } = null!;
    public DateTimeOffset? OriginalReceivedAtUtc { get; private set; }
    private void TrackOriginalReceipt(DateTimeOffset? receivedAtUtc)
    {
        if (BatchId == Guid.Empty || receivedAtUtc is null) return;
        if (OriginalReceivedAtUtc is null || receivedAtUtc < OriginalReceivedAtUtc)
            OriginalReceivedAtUtc = receivedAtUtc;
    }

    public Guid BatchId { get; private set; }
    public string? BatchCode { get; private set; }
    public decimal? PackWeightKg { get; private set; }
    public int? FruitsPerPack { get; private set; }
    public decimal? SellingPrice { get; private set; }
    public string? SellingCurrency { get; private set; }
    public DateTimeOffset? SellingPriceUpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<SellingPriceChange> SellingPriceHistory => _sellingPriceHistory.AsReadOnly();

    public void SetSellingPrice(decimal amount, string currency, Guid userId, string reason, DateTimeOffset now)
    {
        if (OnHand.Value <= 0) throw new InvalidOperationException("Stock must be received before pricing.");
        if (amount <= 0 || amount > 9999999999999999.99m || decimal.Round(amount, 2) != amount)
            throw new InvalidOperationException("Selling price must be positive and have at most two decimal places.");
        currency = currency?.Trim().ToUpperInvariant() ?? "";
        if (currency.Length != 3 || currency.Any(c => c < 'A' || c > 'Z'))
            throw new InvalidOperationException("A three-letter currency code is required.");
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(reason) || reason.Length > 500)
            throw new InvalidOperationException("A user and price change reason are required.");
        if (SellingPrice == amount && SellingCurrency == currency) return;
        _sellingPriceHistory.Add(new SellingPriceChange(SellingPrice, SellingCurrency, amount, currency, userId, reason.Trim(), now));
        SellingPrice = amount;
        SellingCurrency = currency;
        SellingPriceUpdatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public StockQuantity OnHand { get; private set; }
    public StockQuantity Reserved { get; private set; }
    public int Available => OnHand.Value - Reserved.Value;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<StockMovement> Movements => _movements.AsReadOnly();
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    public static InventoryBalance Create(Guid tenantId, Guid warehouseId, Guid productVarietyId, string originCountry, DateTimeOffset createdAtUtc, Guid batchId = default, string? batchCode = null, decimal? packWeightKg = null, int? fruitsPerPack = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));
        if (productVarietyId == Guid.Empty) throw new ArgumentException("Product variety ID cannot be empty.", nameof(productVarietyId));

        if (batchId != Guid.Empty && (string.IsNullOrWhiteSpace(batchCode) || batchCode.Length > 100 || packWeightKg.HasValue == fruitsPerPack.HasValue || packWeightKg is <= 0 || fruitsPerPack is <= 0))
            throw new ArgumentException("Batch code and exactly one positive packaging measurement are required.");
        if (batchId == Guid.Empty && (batchCode is not null || packWeightKg.HasValue || fruitsPerPack.HasValue))
            throw new ArgumentException("Batch identity is required for classified stock.");
        return new InventoryBalance(
            InventoryBalanceId.New(),
            tenantId,
            warehouseId,
            productVarietyId,
            originCountry,
            createdAtUtc) { BatchId = batchId, BatchCode = batchCode, PackWeightKg = packWeightKg, FruitsPerPack = fruitsPerPack };
    }

    public bool Receive(CrateQuantity quantity, StockReference reference, DateTimeOffset occurredAtUtc, DateTimeOffset? originalReceivedAtUtc = null)
    {
        if (
            _movements.Any(
                x =>
                    x.Type == StockMovementType.Received &&
                    x.Reference == reference))
        {
            return false;
        }

        OnHand = new StockQuantity(OnHand.Value + quantity.Value);
        UpdatedAtUtc = occurredAtUtc;

        TrackOriginalReceipt(originalReceivedAtUtc ?? occurredAtUtc);
        AddMovement(StockMovementType.Received, quantity.Value, 0, reference, null, occurredAtUtc);

        RaiseDomainEvent(new StockReceivedDomainEvent(
            Id, TenantId, WarehouseId, ProductVarietyId, quantity.Value, reference.Id, occurredAtUtc));

        return true;
    }

    public bool ReserveForOrder(Guid salesOrderId, CrateQuantity quantity, DateTimeOffset occurredAtUtc)
    {
        var existing = _reservations.SingleOrDefault(x => x.SalesOrderId == salesOrderId);

        if (existing is not null)
        {
            if (existing.Status == StockReservationStatus.Active && existing.Quantity == quantity.Value)
                return false;

            throw new InvalidOperationException("A reservation already exists for this sales order.");
        }

        if (quantity.Value > Available)
            throw new InsufficientStockException(Available, quantity.Value);

        _reservations.Add(new StockReservation(
            StockReservationId.New(), salesOrderId, quantity.Value, occurredAtUtc));

        Reserved = new StockQuantity(Reserved.Value + quantity.Value);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Reserved,
            0,
            quantity.Value,
            StockReference.SalesOrder(salesOrderId),
            null,
            occurredAtUtc);

        RaiseDomainEvent(new StockReservedDomainEvent(
            Id, TenantId, WarehouseId, ProductVarietyId, quantity.Value, salesOrderId, occurredAtUtc));

        return true;
    }

    public bool ReleaseForOrder(Guid salesOrderId, DateTimeOffset occurredAtUtc)
    {
        var reservation = _reservations.SingleOrDefault(x => x.SalesOrderId == salesOrderId);

        if (reservation is null || reservation.Status == StockReservationStatus.Released)
            return false;

        if (reservation.Status == StockReservationStatus.Dispatched)
            throw new InvalidOperationException("A dispatched reservation cannot be released.");

        reservation.Release(occurredAtUtc);
        Reserved = new StockQuantity(Reserved.Value - reservation.Quantity);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.ReservationReleased,
            0,
            -reservation.Quantity,
            StockReference.SalesOrder(salesOrderId),
            null,
            occurredAtUtc);

        RaiseDomainEvent(new StockReservationReleasedDomainEvent(
            Id, TenantId, WarehouseId, ProductVarietyId, reservation.Quantity, salesOrderId, occurredAtUtc));

        return true;
    }

    public bool DispatchForOrder(Guid salesOrderId, DateTimeOffset occurredAtUtc)
    {
        var reservation = _reservations.SingleOrDefault(x => x.SalesOrderId == salesOrderId);

        if (reservation is null) return false;
        if (reservation.Status == StockReservationStatus.Dispatched) return false;
        if (reservation.Status != StockReservationStatus.Active)
            throw new InvalidOperationException("Only an active reservation can be dispatched.");
        if (reservation.Quantity > OnHand.Value)
            throw new InsufficientStockException(OnHand.Value, reservation.Quantity);

        reservation.Dispatch(occurredAtUtc);

        OnHand = new StockQuantity(OnHand.Value - reservation.Quantity);
        Reserved = new StockQuantity(Reserved.Value - reservation.Quantity);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.Dispatched,
            -reservation.Quantity,
            -reservation.Quantity,
            StockReference.SalesOrder(salesOrderId),
            null,
            occurredAtUtc);

        RaiseDomainEvent(new StockDispatchedDomainEvent(
            Id, TenantId, WarehouseId, ProductVarietyId, reservation.Quantity, salesOrderId, occurredAtUtc));

        return true;
    }

    public void ReserveForTransfer(CrateQuantity quantity, StockReference reference, DateTimeOffset now)
    {
        if (quantity.Value > Available) throw new InsufficientStockException(Available, quantity.Value);
        Reserved = new StockQuantity(Reserved.Value + quantity.Value);
        UpdatedAtUtc = now;
        AddMovement(StockMovementType.Reserved, 0, quantity.Value, reference, null, now);
    }

    public void ReleaseTransfer(CrateQuantity quantity, StockReference reference, DateTimeOffset now)
    {
        if (!_movements.Any(m => m.Type == StockMovementType.Reserved && m.Reference == reference && m.ReservedChange == quantity.Value)
            || _movements.Any(m => m.Reference == reference && (m.Type == StockMovementType.ReservationReleased || m.Type == StockMovementType.TransferOut)))
            throw new InvalidOperationException("Transfer reservation is not active.");
        Reserved = new StockQuantity(Reserved.Value - quantity.Value);
        UpdatedAtUtc = now;
        AddMovement(StockMovementType.ReservationReleased, 0, -quantity.Value, reference, null, now);
    }

    public void DispatchTransfer(CrateQuantity quantity, StockReference reference, DateTimeOffset now)
    {
        ReleaseTransfer(quantity, reference, now);
        TransferOut(quantity, reference, null, now);
    }

    public bool TransferOut(
        CrateQuantity quantity,
        StockReference reference,
        string? reason,
        DateTimeOffset occurredAtUtc)
    {
        if (_movements.Any(
                x =>
                    x.Type == StockMovementType.TransferOut &&
                    x.Reference == reference))
        {
            return false;
        }

        if (quantity.Value > Available)
            throw new InsufficientStockException(Available, quantity.Value);

        OnHand = new StockQuantity(OnHand.Value - quantity.Value);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(
            StockMovementType.TransferOut,
            -quantity.Value,
            0,
            reference,
            reason,
            occurredAtUtc);

        return true;
    }

    public bool TransferIn(
        CrateQuantity quantity,
        StockReference reference,
        string? reason,
        DateTimeOffset occurredAtUtc, DateTimeOffset? originalReceivedAtUtc = null)
    {
        if (_movements.Any(
                x =>
                    x.Type == StockMovementType.TransferIn &&
                    x.Reference == reference))
        {
            return false;
        }

        OnHand = new StockQuantity(OnHand.Value + quantity.Value);
        UpdatedAtUtc = occurredAtUtc;

        TrackOriginalReceipt(originalReceivedAtUtc);
        AddMovement(
            StockMovementType.TransferIn,
            quantity.Value,
            0,
            reference,
            reason,
            occurredAtUtc);

        return true;
    }

    public void Adjust(int quantityChange, AdjustmentReason reason, StockReference reference, DateTimeOffset occurredAtUtc)
    {
        if (quantityChange == 0)
            throw new ArgumentException("Adjustment quantity cannot be zero.", nameof(quantityChange));

        var resultingOnHand = OnHand.Value + quantityChange;

        if (resultingOnHand < 0)
            throw new InsufficientStockException(OnHand.Value, Math.Abs(quantityChange));

        if (resultingOnHand < Reserved.Value)
            throw new InvalidOperationException("Adjustment would reduce on-hand stock below reserved stock.");

        OnHand = new StockQuantity(resultingOnHand);
        UpdatedAtUtc = occurredAtUtc;

        AddMovement(StockMovementType.Adjusted, quantityChange, 0, reference, reason.Value, occurredAtUtc);

        RaiseDomainEvent(new StockAdjustedDomainEvent(
            Id, TenantId, WarehouseId, ProductVarietyId, quantityChange, reason.Value, occurredAtUtc));
    }

    private void AddMovement(
        StockMovementType type,
        int onHandChange,
        int reservedChange,
        StockReference reference,
        string? reason,
        DateTimeOffset occurredAtUtc)
    {
        _movements.Add(new StockMovement(
            StockMovementId.New(),
            type,
            onHandChange,
            reservedChange,
            OnHand.Value,
            Reserved.Value,
            reference,
            reason,
            occurredAtUtc));
    }
    private static string NormalizeOriginCountry(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Origin country is required.", nameof(value));

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2)
            throw new ArgumentException("Origin country must be a two-letter country code.", nameof(value));

        return normalized;
    }

}
