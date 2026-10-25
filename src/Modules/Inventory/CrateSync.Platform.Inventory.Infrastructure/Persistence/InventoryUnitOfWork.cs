using CrateSync.Platform.Inventory.Application.Abstractions;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence;

internal sealed class InventoryUnitOfWork(InventoryDbContext dbContext)
    : IInventoryUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
