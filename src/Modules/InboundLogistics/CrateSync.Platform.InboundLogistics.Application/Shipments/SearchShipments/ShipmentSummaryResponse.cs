namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;

public sealed record ShipmentSummaryResponse(
    Guid Id,
    Guid SupplierId,
    string TrackingNumber,
    string OriginCountry,
    string Status,
    DateTimeOffset ExpectedArrivalAtUtc,
    int TotalExpectedCrates,
    int TotalReceivedCrates,
    string? ContainerNumber,
    string? BillOfLadingNumber,
    string? BillOfEntryNumber,
    string CustomsStatus,
    string PlantQuarantineStatus,
    string FoodSafetyStatus);
