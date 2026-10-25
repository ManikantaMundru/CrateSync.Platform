using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.TradingPartners.Contracts.Customers;

public sealed record CustomerCreatedIntegrationEvent : IIntegrationEvent
{
    public CustomerCreatedIntegrationEvent(
        Guid customerId,
        Guid tenantId,
        string name,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        TenantId = tenantId;
        Name = name;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid CustomerId { get; }

    public Guid TenantId { get; }

    public string Name { get; }
}
