using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record AddShipmentDocumentRequest(
    ShipmentDocumentType Type,
    string DocumentName,
    string? ReferenceNumber,
    string? Notes);
