using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record RecordClearanceEventRequest(
    ClearanceAuthority Authority,
    string Status,
    string? ReferenceNumber,
    string? Remarks);
