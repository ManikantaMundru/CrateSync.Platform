namespace CrateSync.Platform.Inventory.Contracts;

public sealed record ReserveStockRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId);
