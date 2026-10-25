using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.TradingPartners.Domain.Customers.Events;

public sealed record CustomerDeactivatedDomainEvent(
    CustomerId CustomerId,
    Guid TenantId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
