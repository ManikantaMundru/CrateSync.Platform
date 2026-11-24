namespace CrateSync.Platform.Api.Requests.InboundShipments;

public sealed record CreateWarehouseDeliveryRequest(
    Guid WarehouseId,
    string TransportCompany,
    string DriverName,
    string DriverPhone,
    string? VehicleNumber,
    string OriginLocation,
    string DestinationLocation,
    string? TransportReference,
    string? Remarks,
    IReadOnlyCollection<CreateWarehouseDeliveryAllocationRequest> Allocations);

public sealed record CreateWarehouseDeliveryAllocationRequest(
    Guid ShipmentLineId,
    int Quantity);
