using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.InboundLogistics.Contracts;

public sealed record ShipmentReceivedIntegrationEvent : IIntegrationEvent
{
    public ShipmentReceivedIntegrationEvent(
        Guid shipmentId,
        Guid tenantId,
        Guid supplierId,
        Guid warehouseId,
        IReadOnlyCollection<ShipmentReceivedLineContract> lines,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        Lines = lines;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid ShipmentId { get; }

    public Guid TenantId { get; }

    public Guid SupplierId { get; }

    public Guid WarehouseId { get; }

    public IReadOnlyCollection<ShipmentReceivedLineContract> Lines { get; }
}
