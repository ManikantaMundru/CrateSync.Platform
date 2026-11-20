using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class StockReservation : Entity<StockReservationId>
{
    private StockReservation() { }

    internal StockReservation(
        StockReservationId id,
        Guid salesOrderId,
        int quantity,
        DateTimeOffset createdAtUtc)
        : base(id)
    {
        if (salesOrderId == Guid.Empty) throw new ArgumentException("Sales order ID cannot be empty.", nameof(salesOrderId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        SalesOrderId = salesOrderId;
        Quantity = quantity;
        Status = StockReservationStatus.Active;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid SalesOrderId { get; private set; }
    public int Quantity { get; private set; }
    public StockReservationStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    internal void Release(DateTimeOffset occurredAtUtc)
    {
        if (Status == StockReservationStatus.Released) return;
        if (Status == StockReservationStatus.Dispatched)
            throw new InvalidOperationException("A dispatched reservation cannot be released.");

        Status = StockReservationStatus.Released;
        CompletedAtUtc = occurredAtUtc;
    }

    internal void Dispatch(DateTimeOffset occurredAtUtc)
    {
        if (Status == StockReservationStatus.Dispatched) return;
        if (Status != StockReservationStatus.Active)
            throw new InvalidOperationException("Only an active reservation can be dispatched.");

        Status = StockReservationStatus.Dispatched;
        CompletedAtUtc = occurredAtUtc;
    }
}
