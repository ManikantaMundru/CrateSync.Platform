using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;

public sealed record ShipmentReceivedDomainEvent(
    InboundShipmentId ShipmentId,
    Guid TenantId,
    Guid SupplierId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
