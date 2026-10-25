using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;

public sealed record CreateShipmentCommand(
    Guid SupplierId,
    Guid WarehouseId,
    string TrackingNumber,
    string OriginCountry,
    DateTimeOffset ExpectedArrivalAtUtc) : ICommand<Result<Guid>>;
