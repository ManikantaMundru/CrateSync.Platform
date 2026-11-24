using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Inventory.Infrastructure.Repositories;

internal sealed class InventoryBalanceRepository(InventoryDbContext dbContext) : IInventoryBalanceRepository
{
    public Task<InventoryBalance?> GetAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        string originCountry,
        CancellationToken cancellationToken = default)
    {
        return dbContext.InventoryBalances
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.WarehouseId == warehouseId &&
                     x.ProductVarietyId == productVarietyId &&
                     x.OriginCountry == originCountry,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryBalance>> GetBySalesOrderIdAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.InventoryBalances
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .Where(x =>
                x.TenantId == tenantId &&
                x.Reservations.Any(r => r.SalesOrderId == salesOrderId))
            .ToListAsync(cancellationToken);
    }

    public void Add(InventoryBalance balance) => dbContext.InventoryBalances.Add(balance);
}
