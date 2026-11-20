namespace CrateSync.Platform.Tenancy.Application.Abstractions;

public interface ITenancyUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
