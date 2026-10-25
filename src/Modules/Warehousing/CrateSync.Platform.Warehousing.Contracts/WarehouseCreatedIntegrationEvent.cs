using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.Warehousing.Contracts.Warehouses;

public sealed record WarehouseCreatedIntegrationEvent : IIntegrationEvent
{
    public WarehouseCreatedIntegrationEvent(
        Guid warehouseId,
        Guid tenantId,
        string name,
        string code,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        TenantId = tenantId;
        Name = name;
        Code = code;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid WarehouseId { get; }

    public Guid TenantId { get; }

    public string Name { get; }

    public string Code { get; }
}
