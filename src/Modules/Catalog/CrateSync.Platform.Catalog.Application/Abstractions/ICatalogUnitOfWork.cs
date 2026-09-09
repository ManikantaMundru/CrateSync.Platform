namespace CrateSync.Platform.Catalog.Application.Abstractions
{
    public interface ICatalogUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
