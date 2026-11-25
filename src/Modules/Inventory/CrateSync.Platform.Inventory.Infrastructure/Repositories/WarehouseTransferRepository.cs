using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CrateSync.Platform.Inventory.Infrastructure.Repositories;
internal sealed class WarehouseTransferRepository(InventoryDbContext db) : IWarehouseTransferRepository
{
    public Task<WarehouseTransfer?> GetAsync(Guid tenantId, Guid id, CancellationToken ct) =>
        db.Set<WarehouseTransfer>().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
    public async Task<IReadOnlyCollection<WarehouseTransfer>> ListAsync(Guid tenantId, CancellationToken ct) =>
        await db.Set<WarehouseTransfer>().AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    public void Add(WarehouseTransfer transfer) => db.Set<WarehouseTransfer>().Add(transfer);
}
