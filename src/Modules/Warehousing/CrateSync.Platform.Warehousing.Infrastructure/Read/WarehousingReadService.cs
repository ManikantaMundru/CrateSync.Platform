using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;
using Dapper;

namespace CrateSync.Platform.Warehousing.Infrastructure.Read;

internal sealed class WarehousingReadService(ISqlConnectionFactory connectionFactory) : IWarehousingReadService
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<WarehouseResponse?> GetByIdAsync(
        Guid tenantId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                Code,
                IsActive,
                AddressLine1,
                AddressLine2,
                City,
                StateOrProvince,
                PostalCode,
                Country,
                CreatedAtUtc,
                UpdatedAtUtc,
                DeactivatedAtUtc
            FROM warehousing.Warehouses
            WHERE Id = @WarehouseId
              AND TenantId = @TenantId;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<WarehouseResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    WarehouseId = warehouseId,
                    TenantId = tenantId
                },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<WarehouseSummaryResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                Code,
                City,
                Country,
                IsActive
            FROM warehousing.Warehouses
            WHERE TenantId = @TenantId
              AND (
                    @SearchTerm IS NULL
                    OR Name LIKE '%' + @SearchTerm + '%'
                    OR Code LIKE '%' + @SearchTerm + '%'
                    OR City LIKE '%' + @SearchTerm + '%'
                  )
              AND (@IsActive IS NULL OR IsActive = @IsActive)
            ORDER BY Name;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<WarehouseSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    SearchTerm = string.IsNullOrWhiteSpace(searchTerm)
                        ? null
                        : searchTerm.Trim(),
                    IsActive = isActive
                },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }
}
