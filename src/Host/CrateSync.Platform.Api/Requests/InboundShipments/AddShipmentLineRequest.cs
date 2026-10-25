namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record AddShipmentLineRequest(
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ExpectedQuantity,
    decimal PackSizeKg,
    decimal UnitCost,
    string Currency);
