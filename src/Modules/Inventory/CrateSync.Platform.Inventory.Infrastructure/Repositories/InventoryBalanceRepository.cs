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
        string countryOfOrigin,
        CancellationToken cancellationToken = default)
    {
        var origin = countryOfOrigin.Trim().ToUpperInvariant();

        return dbContext.InventoryBalances
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.WarehouseId == warehouseId &&
                     x.ProductVarietyId == productVarietyId &&
                     x.CountryOfOrigin == origin,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryBalance>> GetByVarietyAsync(
        Guid tenantId,
        Guid warehouseId,
        Guid productVarietyId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.InventoryBalances
            .Include(x => x.Reservations)
            .Include(x => x.Movements)
            .Where(x =>
                x.TenantId == tenantId &&
                x.WarehouseId == warehouseId &&
                x.ProductVarietyId == productVarietyId)
            .OrderBy(x => x.CountryOfOrigin)
            .ToListAsync(cancellationToken);
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

    public void Add(InventoryBalance balance) =>
        dbContext.InventoryBalances.Add(balance);
}
