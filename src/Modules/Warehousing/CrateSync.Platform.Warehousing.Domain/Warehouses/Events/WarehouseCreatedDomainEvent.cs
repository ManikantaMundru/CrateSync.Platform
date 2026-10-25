using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Warehousing.Domain.Warehouses.Events;

public sealed record WarehouseCreatedDomainEvent(
    WarehouseId WarehouseId,
    Guid TenantId,
    string Name,
    string Code,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
