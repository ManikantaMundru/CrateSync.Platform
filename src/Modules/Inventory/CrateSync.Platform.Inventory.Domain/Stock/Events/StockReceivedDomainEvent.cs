using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Stock.Events;

public sealed record StockReceivedDomainEvent(
    InventoryBalanceId InventoryBalanceId,
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int Quantity,
    Guid ShipmentId,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
