using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Stock.Events;

public sealed record StockDispatchedDomainEvent(
    InventoryBalanceId InventoryBalanceId,
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid SalesOrderId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
