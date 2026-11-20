using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables;
using CrateSync.Platform.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Finance.Infrastructure.Repositories;

internal sealed class SupplierInvoiceRepository(
    FinanceDbContext dbContext)
    : ISupplierInvoiceRepository
{
    public Task<SupplierInvoice?> GetByIdAsync(
        SupplierInvoiceId id,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.SupplierInvoices
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(
                x => x.Id == id &&
                     x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> InvoiceNumberExistsAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        return dbContext.SupplierInvoices.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.InvoiceNumber == invoiceNumber.Trim(),
            cancellationToken);
    }

    public void Add(SupplierInvoice invoice) =>
        dbContext.SupplierInvoices.Add(invoice);
}
