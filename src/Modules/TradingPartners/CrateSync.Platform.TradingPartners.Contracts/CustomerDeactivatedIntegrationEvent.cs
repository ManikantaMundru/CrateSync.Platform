using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.TradingPartners.Contracts.Customers;

public sealed record CustomerDeactivatedIntegrationEvent : IIntegrationEvent
{
    public CustomerDeactivatedIntegrationEvent(
        Guid customerId,
        Guid tenantId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        TenantId = tenantId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid CustomerId { get; }

    public Guid TenantId { get; }
}
