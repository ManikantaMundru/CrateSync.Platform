namespace CrateSync.Platform.Api.Requests.Inventory;

public sealed record AdjustStockRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int QuantityChange,
    string Reason);
