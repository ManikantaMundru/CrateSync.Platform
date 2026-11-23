using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.InboundLogistics;

public sealed record ShipmentArrivedIntegrationEvent : IIntegrationEvent
{
    public ShipmentArrivedIntegrationEvent(
        Guid shipmentId,
        Guid tenantId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        TenantId = tenantId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public Guid ShipmentId { get; }

    public Guid TenantId { get; }
}
