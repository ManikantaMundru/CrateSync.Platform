using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.TradingPartners.Contracts.Suppliers;

public sealed record SupplierDeactivatedIntegrationEvent : IIntegrationEvent
{
    public SupplierDeactivatedIntegrationEvent(
        Guid supplierId,
        Guid tenantId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        SupplierId = supplierId;
        TenantId = tenantId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid SupplierId { get; }

    public Guid TenantId { get; }
}
