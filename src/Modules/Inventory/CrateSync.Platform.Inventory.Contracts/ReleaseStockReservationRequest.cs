namespace CrateSync.Platform.Inventory.Contracts;

public sealed record ReleaseStockReservationRequest(
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId);
