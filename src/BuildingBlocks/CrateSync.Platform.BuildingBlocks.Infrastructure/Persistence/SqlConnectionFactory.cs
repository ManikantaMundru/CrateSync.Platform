using System.Data.Common;
using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using Microsoft.Data.SqlClient;

namespace CrateSync.Platform.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// SqlConnectionFactor is useful because here as all modules one consistent 
/// way to open SQL connections without leaking SqlConnection creation logic everywhere
/// using only on the read side queries
/// </summary>
internal sealed class SqlConnectionFactory: ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
