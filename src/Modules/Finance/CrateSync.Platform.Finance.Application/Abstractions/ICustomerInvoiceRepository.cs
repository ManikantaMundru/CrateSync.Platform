using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;

namespace CrateSync.Platform.Finance.Application.Abstractions;

public interface ICustomerInvoiceRepository
{
    Task<CustomerInvoice?> GetByIdAsync(
        CustomerInvoiceId id,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<CustomerInvoice?> GetBySalesOrderIdAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default);

    Task<bool> InvoiceNumberExistsAsync(
        Guid tenantId,
        string invoiceNumber,
        CancellationToken cancellationToken = default);

    void Add(CustomerInvoice invoice);
}
