namespace CrateSync.Platform.Inventory.Application.Stock.SearchStock;

public sealed record StockSummaryResponse(
    Guid Id,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int OnHand,
    int Reserved,
    int Available,
    DateTimeOffset UpdatedAtUtc);
