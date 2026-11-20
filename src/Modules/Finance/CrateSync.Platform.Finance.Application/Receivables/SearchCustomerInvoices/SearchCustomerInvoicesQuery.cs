using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;

public sealed record SearchCustomerInvoicesQuery(
    InvoiceStatus? Status,
    Guid? CustomerId)
    : IQuery<IReadOnlyCollection<CustomerInvoiceSummaryResponse>>;
