namespace CrateSync.Platform.InboundLogistics.Contracts;

public sealed record ShipmentReceivedLineContract(
    Guid ShipmentLineId,
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ReceivedQuantity,
    decimal PackSizeKg);
