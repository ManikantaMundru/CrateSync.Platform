using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Stock;

public sealed class StockMovement : Entity<StockMovementId>
{
    private StockMovement() { Reference = null!; }

    internal StockMovement(
        StockMovementId id,
        StockMovementType type,
        int onHandChange,
        int reservedChange,
        int onHandAfter,
        int reservedAfter,
        StockReference reference,
        string? reason,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        Type = type;
        OnHandChange = onHandChange;
        ReservedChange = reservedChange;
        OnHandAfter = onHandAfter;
        ReservedAfter = reservedAfter;
        // Each movement owns its reference instance in EF Core. Keep the
        // reference value identical without sharing the tracked owned entity.
        ArgumentNullException.ThrowIfNull(reference);
        Reference = reference with { };
        Reason = reason;
        OccurredAtUtc = occurredAtUtc;
    }

    public StockMovementType Type { get; private set; }
    public int OnHandChange { get; private set; }
    public int ReservedChange { get; private set; }
    public int OnHandAfter { get; private set; }
    public int ReservedAfter { get; private set; }
    public StockReference Reference { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
