using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;

public sealed record ShipmentReceivingStartedDomainEvent(
    InboundShipmentId ShipmentId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
