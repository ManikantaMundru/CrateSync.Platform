using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.InboundLogistics.Domain.Shipments;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SearchShipments;

public sealed record SearchShipmentsQuery(
    string? SearchTerm,
    ShipmentStatus? Status,
    Guid? SupplierId,
    Guid? WarehouseId)
    : IQuery<IReadOnlyCollection<ShipmentSummaryResponse>>;
