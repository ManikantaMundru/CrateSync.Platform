using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;
using CrateSync.Platform.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Finance.Infrastructure.Repositories;

internal sealed class CustomerInvoiceRepository(
    FinanceDbContext dbContext)
    : ICustomerInvoiceRepository
{
    public Task<CustomerInvoice?> GetByIdAsync(
        CustomerInvoiceId id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CustomerInvoices
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(
                x => x.Id == id &&
                     x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<CustomerInvoice?> GetBySalesOrderIdAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CustomerInvoices
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.SalesOrderId == salesOrderId,
                cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        return dbContext.CustomerInvoices.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.InvoiceNumber == invoiceNumber.Trim(),
            cancellationToken);
    }

    public void Add(CustomerInvoice invoice) =>
        dbContext.CustomerInvoices.Add(invoice);
}
