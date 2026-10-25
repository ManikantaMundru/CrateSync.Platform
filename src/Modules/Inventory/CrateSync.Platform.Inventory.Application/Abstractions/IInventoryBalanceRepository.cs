using CrateSync.Platform.Inventory.Domain.Stock;

namespace CrateSync.Platform.Inventory.Application.Abstractions;

public interface IInventoryBalanceRepository
{
    Task<InventoryBalance?> GetAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default);

    void Add(InventoryBalance balance);
}
