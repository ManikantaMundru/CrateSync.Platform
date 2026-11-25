using CrateSync.Platform.Inventory.Domain.Stock;

namespace CrateSync.Platform.Inventory.Application.Abstractions;

public interface IInventoryBalanceRepository
{
    Task<InventoryBalance?> GetAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry,
        CancellationToken cancellationToken = default);

    Task<InventoryBalance?> GetBatchAsync(Guid tenantId, Guid warehouseId, Guid productVarietyId, string originCountry, Guid batchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryBalance>> GetBySalesOrderIdAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default);

    Task<InventoryBalance?> GetByIdAsync(Guid tenantId, Guid balanceId, CancellationToken cancellationToken = default);
    void Add(InventoryBalance balance);
}
