using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Warehousing.Domain.Warehouses.Events;

public sealed record WarehouseActivatedDomainEvent(
    WarehouseId WarehouseId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
