using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.InboundLogistics.Domain.Shipments.Events;

public sealed record ShipmentCreatedDomainEvent(
    InboundShipmentId ShipmentId,
    Guid TenantId,
    Guid SupplierId,
    Guid WarehouseId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
