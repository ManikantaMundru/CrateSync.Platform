using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables;

namespace CrateSync.Platform.Finance.Application.Abstractions;

public interface ISupplierInvoiceRepository
{
    Task<SupplierInvoice?> GetByIdAsync(
        SupplierInvoiceId id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default);

    void Add(SupplierInvoice invoice);
}
