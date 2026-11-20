using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Tenancy.Application.Abstractions;
using CrateSync.Platform.Tenancy.Application.Tenants.GetTenant;
using CrateSync.Platform.Tenancy.Application.Tenants.SearchTenants;
using Dapper;

namespace CrateSync.Platform.Tenancy.Infrastructure.Read;

internal sealed class TenantReadService(
    ISqlConnectionFactory connectionFactory)
    : ITenantReadService
{
    public async Task<TenantResponse?> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CASE Status
                    WHEN 1 THEN 'Active'
                    WHEN 2 THEN 'Inactive'
                END AS Status,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM tenancy.Tenants
            WHERE Id = @TenantId;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TenantResponse>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<TenantSummaryResponse>> SearchAsync(
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CASE Status
                    WHEN 1 THEN 'Active'
                    WHEN 2 THEN 'Inactive'
                END AS Status,
                UpdatedAtUtc
            FROM tenancy.Tenants
            WHERE @Status IS NULL OR Status = @Status
            ORDER BY Name;
            """;

        var status = isActive switch
        {
            true => 1,
            false => 2,
            null => (int?)null
        };

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var rows = await connection.QueryAsync<TenantSummaryResponse>(
            new CommandDefinition(
                sql,
                new { Status = status },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    public async Task<bool> IsActiveAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM tenancy.Tenants
                    WHERE Id = @TenantId
                      AND Status = 1
                )
                THEN 1 ELSE 0 END
                AS bit);
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId },
                cancellationToken: cancellationToken));
    }
}
