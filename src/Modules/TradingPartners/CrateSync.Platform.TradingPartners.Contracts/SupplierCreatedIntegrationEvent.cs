using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.TradingPartners.Contracts.Suppliers;

public sealed record SupplierCreatedIntegrationEvent : IIntegrationEvent
{
    public SupplierCreatedIntegrationEvent(
        Guid supplierId,
        Guid tenantId,
        string name,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        SupplierId = supplierId;
        TenantId = tenantId;
        Name = name;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid SupplierId { get; }

    public Guid TenantId { get; }

    public string Name { get; }
}
