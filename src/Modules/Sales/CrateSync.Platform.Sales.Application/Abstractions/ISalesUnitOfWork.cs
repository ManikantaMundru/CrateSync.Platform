namespace CrateSync.Platform.Sales.Application.Abstractions;

public interface ISalesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
