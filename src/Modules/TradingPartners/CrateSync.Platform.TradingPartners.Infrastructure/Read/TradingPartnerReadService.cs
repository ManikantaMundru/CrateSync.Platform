using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;
using CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;
using CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;
using CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;
using Dapper;

namespace CrateSync.Platform.TradingPartners.Infrastructure.Read;

internal sealed class TradingPartnersReadService(ISqlConnectionFactory connectionFactory) : ITradingPartnersReadService
{
    private readonly ISqlConnectionFactory _connectionFactory = connectionFactory;

    public async Task<CustomerResponse?> GetCustomerByIdAsync(
        Guid tenantId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CommercialIdentifier,
                IsActive,
                AddressLine1,
                AddressLine2,
                City,
                StateOrProvince,
                PostalCode,
                Country,
                ContactName,
                Email,
                Phone
            FROM partners.Customers
            WHERE Id = @CustomerId
              AND TenantId = @TenantId;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<CustomerRow>(
            new CommandDefinition(
                sql,
                new
                {
                    CustomerId = customerId,
                    TenantId = tenantId
                },
                cancellationToken: cancellationToken));

        if (row is null) return null;

        return new CustomerResponse(
            row.Id,
            row.Name,
            row.CommercialIdentifier,
            row.IsActive,
            CreateAddress(row),
            CreateContactDetails(row));
    }

    public async Task<IReadOnlyCollection<CustomerSummaryResponse>> SearchCustomersAsync(
        Guid tenantId,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CommercialIdentifier,
                IsActive
            FROM partners.Customers
            WHERE TenantId = @TenantId
              AND (@SearchTerm IS NULL
                   OR Name LIKE '%' + @SearchTerm + '%'
                   OR CommercialIdentifier LIKE '%' + @SearchTerm + '%')
              AND (@IsActive IS NULL OR IsActive = @IsActive)
            ORDER BY Name;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<CustomerSummaryResponse>(
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

    public async Task<SupplierResponse?> GetSupplierByIdAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CommercialIdentifier,
                IsActive,
                AddressLine1,
                AddressLine2,
                City,
                StateOrProvince,
                PostalCode,
                Country,
                ContactName,
                Email,
                Phone
            FROM partners.Suppliers
            WHERE Id = @SupplierId
              AND TenantId = @TenantId;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<SupplierRow>(
            new CommandDefinition(
                sql,
                new
                {
                    SupplierId = supplierId,
                    TenantId = tenantId
                },
                cancellationToken: cancellationToken));

        if (row is null) return null;

        return new SupplierResponse(
            row.Id,
            row.Name,
            row.CommercialIdentifier,
            row.IsActive,
            CreateAddress(row),
            CreateContactDetails(row));
    }

    public async Task<IReadOnlyCollection<SupplierSummaryResponse>> SearchSuppliersAsync(
        Guid tenantId,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                Id,
                Name,
                CommercialIdentifier,
                IsActive
            FROM partners.Suppliers
            WHERE TenantId = @TenantId
              AND (@SearchTerm IS NULL
                   OR Name LIKE '%' + @SearchTerm + '%'
                   OR CommercialIdentifier LIKE '%' + @SearchTerm + '%')
              AND (@IsActive IS NULL OR IsActive = @IsActive)
            ORDER BY Name;
            """;

        await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<SupplierSummaryResponse>(
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

    private static AddressResponse? CreateAddress(CustomerRow row)
    {
        if (row.AddressLine1 is null) return null;

        return new AddressResponse(
            row.AddressLine1,
            row.AddressLine2,
            row.City!,
            row.StateOrProvince,
            row.PostalCode!,
            row.Country!);
    }

    private static ContactDetailsResponse? CreateContactDetails(CustomerRow row)
    {
        if (row.Email is null && row.Phone is null) return null;

        return new ContactDetailsResponse(
            row.ContactName,
            row.Email,
            row.Phone);
    }

    private static SupplierAddressResponse? CreateAddress(SupplierRow row)
    {
        if (row.AddressLine1 is null) return null;

        return new SupplierAddressResponse(
            row.AddressLine1,
            row.AddressLine2,
            row.City!,
            row.StateOrProvince,
            row.PostalCode!,
            row.Country!);
    }

    private static SupplierContactDetailsResponse? CreateContactDetails(SupplierRow row)
    {
        if (row.Email is null && row.Phone is null) return null;

        return new SupplierContactDetailsResponse(
            row.ContactName,
            row.Email,
            row.Phone);
    }

    private sealed record CustomerRow(
        Guid Id,
        string Name,
        string? CommercialIdentifier,
        bool IsActive,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? StateOrProvince,
        string? PostalCode,
        string? Country,
        string? ContactName,
        string? Email,
        string? Phone);

    private sealed record SupplierRow(
        Guid Id,
        string Name,
        string? CommercialIdentifier,
        bool IsActive,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? StateOrProvince,
        string? PostalCode,
        string? Country,
        string? ContactName,
        string? Email,
        string? Phone);
}
