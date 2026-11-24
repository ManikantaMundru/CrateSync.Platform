namespace CrateSync.Platform.Api.Requests.Inventory;

public sealed record TransferStockRequest(
    Guid TransferId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid ProductVarietyId,
    string CountryOfOrigin,
    int Quantity,
    string? TransferReference,
    string? Remarks);
