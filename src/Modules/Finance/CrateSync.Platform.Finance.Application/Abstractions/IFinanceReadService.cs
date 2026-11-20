using CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;
using CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;
using CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;
using CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Abstractions;

public interface IFinanceReadService
{
    Task<CustomerInvoiceResponse?> GetCustomerInvoiceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CustomerInvoiceSummaryResponse>> SearchCustomerInvoicesAsync(
        Guid tenantId,
        InvoiceStatus? status,
        Guid? customerId,
        CancellationToken cancellationToken = default);

    Task<SupplierInvoiceResponse?> GetSupplierInvoiceAsync(
        Guid tenantId,
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SupplierInvoiceSummaryResponse>> SearchSupplierInvoicesAsync(
        Guid tenantId,
        InvoiceStatus? status,
        Guid? supplierId,
        CancellationToken cancellationToken = default);
}
