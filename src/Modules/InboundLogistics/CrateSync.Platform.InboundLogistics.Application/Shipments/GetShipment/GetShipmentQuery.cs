using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.GetShipment;

public sealed record GetShipmentQuery(
    Guid ShipmentId) : IQuery<Result<ShipmentResponse>>;
