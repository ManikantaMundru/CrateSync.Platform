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
    IReadOnlyCollection<ShipmentDocumentResponse>? Documents = null,
    IReadOnlyCollection<ShipmentReceiptResponse>? Receipts = null,
    IReadOnlyCollection<ShipmentDeliveryResponse>? Deliveries = null,
    bool IsClearedForWarehouseDispatch = false,
    IReadOnlyCollection<RegulatorySampleResponse>? RegulatorySamples = null);

public sealed record ShipmentLineResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ExpectedQuantity,
    int ReceivedQuantity,
    decimal? PackSizeKg,
    decimal? UnitCost,
    string? Currency,
    int? FruitsPerPack,
    int RegulatorySampleQuantity = 0,
    int DamagedQuantity = 0,
    int MissingQuantity = 0,
    int PendingAllocationQuantity = 0)
{
    public int AccountedQuantity => ReceivedQuantity + RegulatorySampleQuantity + DamagedQuantity + MissingQuantity;
    public int RemainingQuantity => Math.Max(0, ExpectedQuantity - AccountedQuantity);
    public int AvailableForAllocation => Math.Max(0, RemainingQuantity - PendingAllocationQuantity);
}

public sealed record RegulatorySampleResponse(Guid Id, Guid ShipmentLineId, string Authority, int Quantity, string? ReferenceNumber, string Remarks, DateTimeOffset RecordedAtUtc, DateTimeOffset? ReversedAtUtc = null, string? ReversalReason = null);

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


public sealed record ShipmentReceiptResponse(
    Guid Id,
    Guid ShipmentLineId,
    Guid WarehouseId,
    Guid? DeliveryId,
    Guid ProductVarietyId,
    int Quantity,
    DateTimeOffset ReceivedAtUtc);

public sealed record ShipmentDeliveryResponse(
    Guid Id,
    Guid WarehouseId,
    string Status,
    string TransportCompany,
    string DriverName,
    string DriverPhone,
    string? VehicleNumber,
    string OriginLocation,
    string DestinationLocation,
    string? TransportReference,
    string? Remarks,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DispatchedAtUtc,
    DateTimeOffset? ReceivedAtUtc,
    IReadOnlyCollection<ShipmentDeliveryAllocationResponse> Allocations);

public sealed record ShipmentDeliveryAllocationResponse(
    Guid Id,
    Guid DeliveryId,
    Guid ShipmentLineId,
    int Quantity,
    int? AcceptedQuantity = null,
    int? DamagedQuantity = null,
    int? MissingQuantity = null,
    string? ReceivingRemarks = null);
