using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.Inventory.Contracts;

public sealed record StockReservedIntegrationEvent : IIntegrationEvent
{
    public StockReservedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        Guid warehouseId,
        Guid productVarietyId,
        int quantity,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        WarehouseId = warehouseId;
        ProductVarietyId = productVarietyId;
        Quantity = quantity;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public Guid WarehouseId { get; }

    public Guid ProductVarietyId { get; }

    public int Quantity { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
