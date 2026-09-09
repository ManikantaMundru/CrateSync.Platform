using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Catalog.Application.Abstractions;
using CrateSync.Platform.Catalog.Application.Products.GetProduct;
using CrateSync.Platform.Catalog.Application.Products.SearchProducts;
using CrateSync.Platform.Catalog.Infrastructure.Read.Models;
using Dapper;

namespace CrateSync.Platform.Catalog.Infrastructure.Read;

internal sealed class ProductReadService(ISqlConnectionFactory connectionFactory): IProductReadService
{
    public async Task<ProductResponse?> GetByIdAsync(
        Guid tenantId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                            SELECT
                                p.Id,
                                p.Name,
                                p.IsActive
                            FROM catalog.Products p
                            WHERE
                                p.TenantId = @TenantId
                                AND p.Id = @ProductId;

                            SELECT
                                pv.Id,
                                pv.Name,
                                pv.IsActive
                            FROM catalog.ProductVarieties pv
                            INNER JOIN catalog.Products p
                                ON p.Id = pv.ProductId
                            WHERE
                                p.TenantId = @TenantId
                                AND pv.ProductId = @ProductId
                            ORDER BY
                                pv.Name;
                            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        using var multi =
            await connection.QueryMultipleAsync(
                sql,
                new
                {
                    TenantId = tenantId,
                    ProductId = productId
                });

        var product =
            await multi.ReadSingleOrDefaultAsync<ProductHeader>();

        if (product is null)
        {
            return null;
        }

        var varieties =
            (await multi.ReadAsync<ProductVarietyResponse>())
            .AsList();

        return new ProductResponse(
            product.Id,
            product.Name,
            product.IsActive,
            varieties);
    }

    public async Task<IReadOnlyCollection<ProductSearchResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                            SELECT
                                p.Id,
                                p.Name,
                                p.IsActive,
                                COUNT(pv.Id) AS VarietyCount
                            FROM catalog.Products p
                            LEFT JOIN catalog.ProductVarieties pv
                                ON pv.ProductId = p.Id
                            WHERE
                                p.TenantId = @TenantId
                                AND (@SearchTerm IS NULL OR p.Name LIKE '%' + @SearchTerm + '%')
                                AND (@IsActive IS NULL OR p.IsActive = @IsActive)
                            GROUP BY
                                p.Id,
                                p.Name,
                                p.IsActive
                            ORDER BY
                                p.Name;
                            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var result =
            await connection.QueryAsync<ProductSearchResponse>(
                sql,
                new
                {
                    TenantId = tenantId,
                    SearchTerm = searchTerm,
                    IsActive = isActive
                });

        return result.AsList();
    }
}
