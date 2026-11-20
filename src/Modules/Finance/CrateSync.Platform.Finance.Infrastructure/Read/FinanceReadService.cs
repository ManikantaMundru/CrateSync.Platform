using CrateSync.Platform.BuildingBlocks.Application.Persistence;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;
using CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;
using CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;
using CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;
using CrateSync.Platform.Finance.Domain.Common;
using Dapper;

namespace CrateSync.Platform.Finance.Infrastructure.Read;

internal sealed class FinanceReadService(
    ISqlConnectionFactory connectionFactory)
    : IFinanceReadService
{
    public async Task<CustomerInvoiceResponse?> GetCustomerInvoiceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        const string invoiceSql = """
            SELECT
                Id,
                CustomerId,
                SalesOrderId,
                InvoiceNumber,
                Currency,
                CASE Status
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Partial'
                    WHEN 3 THEN 'Paid'
                    WHEN 4 THEN 'Void'
                END AS Status,
                InvoiceDateUtc,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM finance.CustomerInvoices
            WHERE TenantId = @TenantId
              AND Id = @InvoiceId;
            """;

        const string linesSql = """
            SELECT
                Id,
                ProductVarietyId,
                Quantity,
                PackSize,
                UnitPrice,
                Quantity * UnitPrice AS LineTotal
            FROM finance.CustomerInvoiceLines
            WHERE CustomerInvoiceId = @InvoiceId
            ORDER BY Id;
            """;

        const string paymentsSql = """
            SELECT
                Id,
                Amount,
                Reference,
                PaidAtUtc
            FROM finance.CustomerPayments
            WHERE CustomerInvoiceId = @InvoiceId
            ORDER BY PaidAtUtc;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var invoice = await connection.QuerySingleOrDefaultAsync<CustomerInvoiceRow>(
            new CommandDefinition(
                invoiceSql,
                new
                {
                    TenantId = tenantId,
                    InvoiceId = invoiceId
                },
                cancellationToken: cancellationToken));

        if (invoice is null)
            return null;

        var lines = (await connection.QueryAsync<CustomerInvoiceLineResponse>(
            new CommandDefinition(
                linesSql,
                new { InvoiceId = invoiceId },
                cancellationToken: cancellationToken))).AsList();

        var payments = (await connection.QueryAsync<CustomerPaymentResponse>(
            new CommandDefinition(
                paymentsSql,
                new { InvoiceId = invoiceId },
                cancellationToken: cancellationToken))).AsList();

        var total = lines.Sum(x => x.LineTotal);
        var paid = payments.Sum(x => x.Amount);

        return new CustomerInvoiceResponse(
            invoice.Id,
            invoice.CustomerId,
            invoice.SalesOrderId,
            invoice.InvoiceNumber,
            invoice.Currency,
            invoice.Status,
            total,
            paid,
            total - paid,
            invoice.InvoiceDateUtc,
            invoice.CreatedAtUtc,
            invoice.UpdatedAtUtc,
            lines,
            payments);
    }

    public async Task<IReadOnlyCollection<CustomerInvoiceSummaryResponse>>
        SearchCustomerInvoicesAsync(
            Guid tenantId,
            InvoiceStatus? status,
            Guid? customerId,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                i.Id,
                i.CustomerId,
                i.SalesOrderId,
                i.InvoiceNumber,
                i.Currency,
                i.Status,
                COALESCE(lines.TotalAmount, 0) AS TotalAmount,
                COALESCE(payments.AmountPaid, 0) AS AmountPaid,
                COALESCE(lines.TotalAmount, 0) - COALESCE(payments.AmountPaid, 0) AS OutstandingAmount,
                i.InvoiceDateUtc
            FROM finance.CustomerInvoices i
            OUTER APPLY (
                SELECT SUM(l.Quantity * l.UnitPrice) AS TotalAmount
                FROM finance.CustomerInvoiceLines l
                WHERE l.CustomerInvoiceId = i.Id
            ) lines
            OUTER APPLY (
                SELECT SUM(p.Amount) AS AmountPaid
                FROM finance.CustomerPayments p
                WHERE p.CustomerInvoiceId = i.Id
            ) payments
            WHERE i.TenantId = @TenantId
              AND (@Status IS NULL OR i.Status = @Status)
              AND (@CustomerId IS NULL OR i.CustomerId = @CustomerId)
            ORDER BY i.InvoiceDateUtc DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var rows = await connection.QueryAsync<CustomerInvoiceSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    Status = status is null
                        ? null
                        : (int?)status.Value,
                    CustomerId = customerId
                },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    public async Task<SupplierInvoiceResponse?> GetSupplierInvoiceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        const string invoiceSql = """
            SELECT
                Id,
                SupplierId,
                ShipmentId,
                InvoiceNumber,
                Currency,
                CASE Status
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Partial'
                    WHEN 3 THEN 'Paid'
                    WHEN 4 THEN 'Void'
                END AS Status,
                InvoiceDateUtc,
                CreatedAtUtc,
                UpdatedAtUtc
            FROM finance.SupplierInvoices
            WHERE TenantId = @TenantId
              AND Id = @InvoiceId;
            """;

        const string linesSql = """
            SELECT
                Id,
                Description,
                ProductVarietyId,
                Quantity,
                UnitCost,
                Quantity * UnitCost AS LineTotal
            FROM finance.SupplierInvoiceLines
            WHERE SupplierInvoiceId = @InvoiceId
            ORDER BY Id;
            """;

        const string paymentsSql = """
            SELECT
                Id,
                Amount,
                Reference,
                PaidAtUtc
            FROM finance.SupplierPayments
            WHERE SupplierInvoiceId = @InvoiceId
            ORDER BY PaidAtUtc;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var invoice = await connection.QuerySingleOrDefaultAsync<SupplierInvoiceRow>(
            new CommandDefinition(
                invoiceSql,
                new
                {
                    TenantId = tenantId,
                    InvoiceId = invoiceId
                },
                cancellationToken: cancellationToken));

        if (invoice is null)
            return null;

        var lines = (await connection.QueryAsync<SupplierInvoiceLineResponse>(
            new CommandDefinition(
                linesSql,
                new { InvoiceId = invoiceId },
                cancellationToken: cancellationToken))).AsList();

        var payments = (await connection.QueryAsync<SupplierPaymentResponse>(
            new CommandDefinition(
                paymentsSql,
                new { InvoiceId = invoiceId },
                cancellationToken: cancellationToken))).AsList();

        var total = lines.Sum(x => x.LineTotal);
        var paid = payments.Sum(x => x.Amount);

        return new SupplierInvoiceResponse(
            invoice.Id,
            invoice.SupplierId,
            invoice.ShipmentId,
            invoice.InvoiceNumber,
            invoice.Currency,
            invoice.Status,
            total,
            paid,
            total - paid,
            invoice.InvoiceDateUtc,
            invoice.CreatedAtUtc,
            invoice.UpdatedAtUtc,
            lines,
            payments);
    }

    public async Task<IReadOnlyCollection<SupplierInvoiceSummaryResponse>>
        SearchSupplierInvoicesAsync(
            Guid tenantId,
            InvoiceStatus? status,
            Guid? supplierId,
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                i.Id,
                i.SupplierId,
                i.ShipmentId,
                i.InvoiceNumber,
                i.Currency,
                i.Status,
                COALESCE(lines.TotalAmount, 0) AS TotalAmount,
                COALESCE(payments.AmountPaid, 0) AS AmountPaid,
                COALESCE(lines.TotalAmount, 0) - COALESCE(payments.AmountPaid, 0) AS OutstandingAmount,
                i.InvoiceDateUtc
            FROM finance.SupplierInvoices i
            OUTER APPLY (
                SELECT SUM(l.Quantity * l.UnitCost) AS TotalAmount
                FROM finance.SupplierInvoiceLines l
                WHERE l.SupplierInvoiceId = i.Id
            ) lines
            OUTER APPLY (
                SELECT SUM(p.Amount) AS AmountPaid
                FROM finance.SupplierPayments p
                WHERE p.SupplierInvoiceId = i.Id
            ) payments
            WHERE i.TenantId = @TenantId
              AND (@Status IS NULL OR i.Status = @Status)
              AND (@SupplierId IS NULL OR i.SupplierId = @SupplierId)
            ORDER BY i.InvoiceDateUtc DESC;
            """;

        await using var connection =
            await connectionFactory.OpenConnectionAsync(
                cancellationToken);

        var rows = await connection.QueryAsync<SupplierInvoiceSummaryResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    TenantId = tenantId,
                    Status = status is null
                        ? null
                        : (int?)status.Value,
                    SupplierId = supplierId
                },
                cancellationToken: cancellationToken));

        return rows.AsList();
    }

    private sealed record CustomerInvoiceRow(
        Guid Id,
        Guid CustomerId,
        Guid SalesOrderId,
        string InvoiceNumber,
        string Currency,
        string Status,
        DateTimeOffset InvoiceDateUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);

    private sealed record SupplierInvoiceRow(
        Guid Id,
        Guid SupplierId,
        Guid ShipmentId,
        string InvoiceNumber,
        string Currency,
        string Status,
        DateTimeOffset InvoiceDateUtc,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
