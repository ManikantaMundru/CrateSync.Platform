using CrateSync.Platform.BuildingBlocks.Domain;

namespace CrateSync.Platform.Inventory.Domain.Stock.Events;

public sealed record StockAdjustedDomainEvent(
    InventoryBalanceId InventoryBalanceId,
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductVarietyId,
    int QuantityChange,
    string Reason,
    DateTimeOffset OccurredOnUtc)
    : DomainEvent(OccurredOnUtc);
