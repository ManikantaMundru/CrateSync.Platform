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
    IReadOnlyCollection<ShipmentLineResponse> Lines);

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
