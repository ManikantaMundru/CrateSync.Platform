namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record CreateInboundShipmentRequest(
    Guid SupplierId,
    string TrackingNumber,
    string OriginCountry,
    DateTimeOffset ExpectedArrivalAtUtc);
