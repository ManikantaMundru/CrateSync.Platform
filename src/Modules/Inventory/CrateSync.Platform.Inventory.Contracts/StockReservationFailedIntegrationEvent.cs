using CrateSync.Platform.BuildingBlocks.Application.Events;

namespace CrateSync.Platform.Inventory.Contracts;

public sealed record StockReservationFailedIntegrationEvent : IIntegrationEvent
{
    public StockReservationFailedIntegrationEvent(
        Guid tenantId,
        Guid salesOrderId,
        Guid warehouseId,
        Guid productVarietyId,
        int requestedQuantity,
        string reason,
        DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SalesOrderId = salesOrderId;
        WarehouseId = warehouseId;
        ProductVarietyId = productVarietyId;
        RequestedQuantity = requestedQuantity;
        Reason = reason;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid Id { get; }

    public Guid TenantId { get; }

    public Guid SalesOrderId { get; }

    public Guid WarehouseId { get; }

    public Guid ProductVarietyId { get; }

    public int RequestedQuantity { get; }

    public string Reason { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
