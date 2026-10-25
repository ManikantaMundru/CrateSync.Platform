using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.Warehousing.Contracts.Warehouses;

public sealed record WarehouseDeactivatedIntegrationEvent : IIntegrationEvent
{
    public WarehouseDeactivatedIntegrationEvent(
        Guid warehouseId,
        Guid tenantId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        TenantId = tenantId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid WarehouseId { get; }

    public Guid TenantId { get; }
}
