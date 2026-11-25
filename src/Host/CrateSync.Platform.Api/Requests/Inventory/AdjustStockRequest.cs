namespace CrateSync.Platform.Api.Requests.Inventory;

public sealed record AdjustStockRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int QuantityChange,
    string Reason, Guid BatchId = default);
