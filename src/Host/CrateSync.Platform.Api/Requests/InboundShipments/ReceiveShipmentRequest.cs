namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record ReceiveShipmentRequest(
    Guid ReceiptId,
    Guid ShipmentLineId,
    int Quantity);
