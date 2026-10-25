namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;

public sealed record ShipmentSummaryResponse(
    Guid Id,
    Guid SupplierId,
    Guid WarehouseId,
    string TrackingNumber,
    string OriginCountry,
    string Status,
    DateTimeOffset ExpectedArrivalAtUtc,
    int TotalExpectedCrates,
    int TotalReceivedCrates);
