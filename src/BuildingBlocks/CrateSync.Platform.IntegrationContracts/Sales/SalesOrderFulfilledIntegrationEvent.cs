using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.Sales;

public sealed record SalesOrderFulfilledIntegrationEvent
    : IIntegrationEvent
{
    public SalesOrderFulfilledIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        Guid customerId,
        string orderNumber,
        IReadOnlyCollection<SalesOrderFulfilledLineIntegrationContract> lines,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Lines = lines;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public Guid CustomerId { get; }

    public string OrderNumber { get; }

    public IReadOnlyCollection<SalesOrderFulfilledLineIntegrationContract> Lines { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
