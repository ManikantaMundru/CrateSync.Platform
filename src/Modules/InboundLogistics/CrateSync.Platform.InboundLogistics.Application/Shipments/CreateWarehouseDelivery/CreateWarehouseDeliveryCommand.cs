using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateWarehouseDelivery;

public sealed record CreateWarehouseDeliveryCommand(
    Guid ShipmentId,
    Guid WarehouseId,
    string TransportCompany,
    string DriverName,
    string DriverPhone,
    string? VehicleNumber,
    string OriginLocation,
    string DestinationLocation,
    string? TransportReference,
    string? Remarks,
    IReadOnlyCollection<CreateWarehouseDeliveryAllocation> Allocations)
    : ICommand<Result<Guid>>;

public sealed record CreateWarehouseDeliveryAllocation(
    Guid ShipmentLineId,
    int Quantity);
