namespace CrateSync.Platform.Inventory.Contracts;

public sealed record DispatchStockRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId);
