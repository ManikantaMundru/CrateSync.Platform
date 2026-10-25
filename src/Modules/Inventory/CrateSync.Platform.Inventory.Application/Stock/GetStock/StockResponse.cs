namespace CrateSync.Platform.Inventory.Application.Stock.GetStock;

public sealed record StockResponse(
    Guid Id,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int OnHand,
    int Reserved,
    int Available,
    DateTimeOffset UpdatedAtUtc);
