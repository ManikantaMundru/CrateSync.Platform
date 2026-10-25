namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record CreateInboundShipmentRequest(
    Guid SupplierId,
    Guid WarehouseId,
    string TrackingNumber,
    string OriginCountry,
    DateTimeOffset ExpectedArrivalAtUtc);
