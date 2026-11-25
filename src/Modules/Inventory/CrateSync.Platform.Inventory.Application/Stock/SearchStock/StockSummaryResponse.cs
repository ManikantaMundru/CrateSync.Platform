namespace CrateSync.Platform.Inventory.Application.Stock.SearchStock;

public sealed record StockSummaryResponse(
    Guid Id,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int OnHand,
    int Reserved,
    int Available,
    DateTimeOffset UpdatedAtUtc,
    Guid ProductId = default,
    string ProductName = "",
    string VarietyName = "",
    string WarehouseName = "",
    string WarehouseCode = "",
    string OriginCountry = "", Guid BatchId = default, string? BatchCode = null, decimal? PackWeightKg = null, int? FruitsPerPack = null, decimal? SellingPrice = null, string? SellingCurrency = null, DateTimeOffset? SellingPriceUpdatedAtUtc = null);
