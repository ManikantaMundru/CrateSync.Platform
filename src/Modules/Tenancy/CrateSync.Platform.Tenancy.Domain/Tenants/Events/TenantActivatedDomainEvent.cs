using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Tenancy.Domain.Tenants.Events;

public sealed record TenantActivatedDomainEvent(
    TenantId TenantId,
    DateTimeOffset OccurredAtUtc)
    : DomainEvent(OccurredAtUtc);
