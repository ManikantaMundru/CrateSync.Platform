namespace CrateSync.Platform.Api.Requests.Inventory;

public sealed record AdjustStockRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    string CountryOfOrigin,
    int QuantityChange,
    string Reason);
