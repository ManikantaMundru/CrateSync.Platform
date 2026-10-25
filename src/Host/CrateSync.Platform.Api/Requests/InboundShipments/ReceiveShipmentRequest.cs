namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record ReceiveShipmentRequest(
    Guid ShipmentLineId,
    int Quantity);
