namespace CrateSync.Platform.Warehousing.Application.Abstractions;

public interface IWarehousingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
