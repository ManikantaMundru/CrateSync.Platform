namespace CrateSync.Platform.Inventory.Application.Stock.GetStock;

public sealed record StockResponse(
    Guid Id,
    Guid WarehouseId,
    Guid ProductVarietyId,
    string CountryOfOrigin,
    int OnHand,
    int Reserved,
    int Available,
    DateTimeOffset UpdatedAtUtc,
    Guid ProductId = default,
    string ProductName = "",
    string VarietyName = "",
    string WarehouseName = "",
    string WarehouseCode = "");
