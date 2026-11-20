using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Tenancy.Domain.Tenants.Events;

public sealed record TenantDeactivatedDomainEvent(
    TenantId TenantId,
    DateTimeOffset OccurredAtUtc)
    : DomainEvent(OccurredAtUtc);
