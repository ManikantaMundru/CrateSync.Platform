using CrateSync.Platform.InboundLogistics.Application.Abstractions;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;

internal sealed class InboundLogisticsUnitOfWork(InboundLogisticsDbContext dbContext) : IInboundLogisticsUnitOfWork
{
    private readonly InboundLogisticsDbContext _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
