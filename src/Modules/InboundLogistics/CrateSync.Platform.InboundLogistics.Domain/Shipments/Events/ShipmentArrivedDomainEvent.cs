using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;

public sealed record ShipmentArrivedDomainEvent(
    InboundShipmentId ShipmentId,
    Guid TenantId,
    Guid WarehouseId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
