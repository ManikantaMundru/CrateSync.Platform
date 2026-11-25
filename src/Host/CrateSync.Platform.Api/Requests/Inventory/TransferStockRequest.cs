namespace CrateSync.Platform.Api.Requests.Inventory;

public sealed record TransferStockRequest(
    Guid TransferId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid ProductVarietyId,
    string OriginCountry,
    int Quantity,
    string? TransferReference,
    string? Remarks, Guid BatchId = default, string TransportCompany = "", string DriverName = "", string DriverPhone = "", string? VehicleNumber = null);
