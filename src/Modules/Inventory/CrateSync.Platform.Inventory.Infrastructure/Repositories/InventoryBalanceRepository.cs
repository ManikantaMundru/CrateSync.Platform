using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Inventory.Infrastructure.Repositories;

internal sealed class InventoryBalanceRepository(
    InventoryDbContext dbContext)
    : IInventoryBalanceRepository
{
    public Task<InventoryBalance?> GetAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.InventoryBalances
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.WarehouseId == warehouseId &&
                    x.ProductVarietyId == productVarietyId,
                cancellationToken);
    }

    public void Add(InventoryBalance balance)
    {
        dbContext.InventoryBalances.Add(balance);
    }
}
