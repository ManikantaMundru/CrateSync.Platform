using CrateSync.Platform.Tenancy.Application.Abstractions;

namespace CrateSync.Platform.Tenancy.Infrastructure.Persistence;

internal sealed class TenancyUnitOfWork(
    TenancyDbContext dbContext)
    : ITenancyUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
