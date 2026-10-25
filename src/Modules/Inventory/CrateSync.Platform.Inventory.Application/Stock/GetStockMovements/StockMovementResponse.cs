namespace CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;

public sealed record StockMovementResponse(
    Guid Id,
    string Type,
    int OnHandChange,
    int ReservedChange,
    int OnHandAfter,
    int ReservedAfter,
    string ReferenceType,
    Guid ReferenceId,
    string? Reason,
    DateTimeOffset OccurredAtUtc);
