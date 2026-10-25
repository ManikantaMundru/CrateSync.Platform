using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.TradingPartners.Domain.Customers.Events;

public sealed record CustomerCreatedDomainEvent(
    CustomerId CustomerId,
    Guid TenantId,
    string Name,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
