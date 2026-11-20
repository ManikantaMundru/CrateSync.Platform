using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Tenancy.Domain.Tenants.Events;

public sealed record TenantCreatedDomainEvent(
    TenantId TenantId,
    string Name,
    DateTimeOffset OccurredAtUtc)
    : DomainEvent(OccurredAtUtc);
