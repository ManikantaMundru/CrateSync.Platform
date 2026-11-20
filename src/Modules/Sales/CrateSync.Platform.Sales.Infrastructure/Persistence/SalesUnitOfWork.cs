using CrateSync.Platform.Sales.Application.Abstractions;

namespace CrateSync.Platform.Sales.Infrastructure.Persistence;

internal sealed class SalesUnitOfWork(SalesDbContext dbContext) : ISalesUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
