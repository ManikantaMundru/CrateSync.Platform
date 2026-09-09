using System.Data.Common;

namespace CrateSync.Platform.BuildingBlocks.Application.Persistence;

public interface ISqlConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
