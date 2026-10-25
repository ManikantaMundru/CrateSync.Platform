using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Domain.Warehouses;
using CrateSync.Platform.Warehousing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Warehousing.Infrastructure.Repositories;

internal sealed class WarehouseRepository(WarehousingDbContext dbContext) : IWarehouseRepository
{
    private readonly WarehousingDbContext _dbContext = dbContext;

    public Task<Warehouse?> GetByIdAsync(
        WarehouseId warehouseId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .FirstOrDefaultAsync(
                x => x.Id == warehouseId && x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        Guid tenantId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Warehouses
            .AnyAsync(
                x => x.TenantId == tenantId && x.Code == code,
                cancellationToken);
    }

    public void Add(Warehouse warehouse)
    {
        _dbContext.Warehouses.Add(warehouse);
    }
}
