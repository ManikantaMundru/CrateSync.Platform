using CrateSync.Platform.Finance.Application.Abstractions;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence;

internal sealed class FinanceUnitOfWork(
    FinanceDbContext dbContext)
    : IFinanceUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
