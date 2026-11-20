using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.IntegrationContracts.Sales;

public sealed record SalesOrderConfirmedIntegrationEvent
    : IIntegrationEvent
{
    public SalesOrderConfirmedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        Guid warehouseId,
        IReadOnlyCollection<SalesOrderLineIntegrationContract> lines,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        WarehouseId = warehouseId;
        Lines = lines;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public Guid WarehouseId { get; }

    public IReadOnlyCollection<SalesOrderLineIntegrationContract> Lines { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
