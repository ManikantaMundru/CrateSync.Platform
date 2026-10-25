using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.InboundLogistics.Contracts;

public sealed record ShipmentArrivedIntegrationEvent : IIntegrationEvent
{
    public ShipmentArrivedIntegrationEvent(
        Guid shipmentId,
        Guid tenantId,
        Guid warehouseId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        TenantId = tenantId;
        WarehouseId = warehouseId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid ShipmentId { get; }

    public Guid TenantId { get; }

    public Guid WarehouseId { get; }
}
