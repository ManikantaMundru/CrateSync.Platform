using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Warehousing.Domain.Warehouses.Events;

public sealed record WarehouseDeactivatedDomainEvent(
    WarehouseId WarehouseId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
