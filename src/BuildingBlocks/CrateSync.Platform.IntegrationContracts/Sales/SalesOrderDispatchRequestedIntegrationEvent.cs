using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.Sales;

public sealed record SalesOrderDispatchRequestedIntegrationEvent(
    Guid Id,
    Guid TenantId,
    Guid SalesOrderId,
    DateTimeOffset OccurredAtUtc)
    : IIntegrationEvent
{
    public SalesOrderDispatchRequestedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        DateTimeOffset occurredAtUtc)
        : this(
            Guid.NewGuid(),
            tenantId,
            salesOrderId,
            occurredAtUtc)
    {
    }
}
