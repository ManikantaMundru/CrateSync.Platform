using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.Inventory;

public sealed record StockReservedIntegrationEvent : IIntegrationEvent
{
    public StockReservedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
