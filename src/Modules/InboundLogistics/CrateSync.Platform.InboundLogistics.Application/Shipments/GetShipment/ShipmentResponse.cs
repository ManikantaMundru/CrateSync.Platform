namespace CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;

public sealed record ShipmentResponse(
    Guid Id,
    Guid SupplierId,
    Guid WarehouseId,
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
    string? ContainerNumber,
    string? BillOfLadingNumber,
    string? PortOfLoading,
    string? PortOfDischarge,
    string? VesselName,
    string? VoyageNumber,
    string? CustomsBrokerName,
    string? BillOfEntryNumber,
    DateTimeOffset? BillOfEntryDate,
    DateTimeOffset? OutOfChargeAtUtc,
    string CustomsStatus,
    string PlantQuarantineStatus,
    string FoodSafetyStatus,
    IReadOnlyCollection<ShipmentLineResponse> Lines,
    IReadOnlyCollection<ShipmentClearanceEventResponse> ClearanceEvents,
    IReadOnlyCollection<ShipmentDocumentResponse> Documents);

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
