using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.Inventory;

public sealed record StockReservationFailedIntegrationEvent : IIntegrationEvent
{
    public StockReservationFailedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        string reason,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        Reason = reason;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public string Reason { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
