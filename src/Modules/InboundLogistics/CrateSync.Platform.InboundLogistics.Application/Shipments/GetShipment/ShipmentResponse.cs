namespace CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;

public sealed record ShipmentResponse(
    Guid Id,
    Guid SupplierId,
    string TrackingNumber,
    string OriginCountry,
    string Status,
    DateTimeOffset ExpectedArrivalAtUtc,
    DateTimeOffset? ShippedAtUtc,
    DateTimeOffset? ArrivedAtUtc,
    DateTimeOffset? ReceivedAtUtc,
    DateTimeOffset? CancelledAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<ShipmentLineResponse> Lines,
    string? ContainerNumber = null,
    string? BillOfLadingNumber = null,
    string? PortOfLoading = null,
    string? PortOfDischarge = null,
    string? VesselName = null,
    string? VoyageNumber = null,
    string? CustomsBrokerName = null,
    string? BillOfEntryNumber = null,
    DateTimeOffset? BillOfEntryDate = null,
    DateTimeOffset? OutOfChargeAtUtc = null,
    string CustomsStatus = "NotStarted",
    string PlantQuarantineStatus = "NotRequired",
    string FoodSafetyStatus = "NotRequired",
    IReadOnlyCollection<ShipmentClearanceEventResponse>? ClearanceEvents = null,
    IReadOnlyCollection<ShipmentDocumentResponse>? Documents = null);

public sealed record ShipmentLineResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ExpectedQuantity,
    int ReceivedQuantity,
    decimal PackSizeKg,
    decimal UnitCost,
    string Currency);

public sealed record ShipmentClearanceEventResponse(
    Guid Id,
    string Authority,
    string Status,
    string? ReferenceNumber,
    string? Remarks,
    DateTimeOffset OccurredAtUtc);

public sealed record ShipmentDocumentResponse(
    Guid Id,
    string Type,
    string DocumentName,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreatedAtUtc);
