namespace CrateSync.Platform.IntegrationContracts.InboundLogistics;

public sealed record ShipmentReceivedLineContract(
    Guid ShipmentLineId,
    Guid ProductId,
    Guid ProductVarietyId,
    string BatchCode,
    int ReceivedQuantity,
    decimal PackSizeKg);
