namespace CrateSync.Platform.Finance.Application.Abstractions;

public interface IFinanceUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
