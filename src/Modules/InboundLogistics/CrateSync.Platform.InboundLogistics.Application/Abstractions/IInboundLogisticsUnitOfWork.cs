namespace CrateSync.Platform.InboundLogistics.Application.Abstractions;

public interface IInboundLogisticsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
