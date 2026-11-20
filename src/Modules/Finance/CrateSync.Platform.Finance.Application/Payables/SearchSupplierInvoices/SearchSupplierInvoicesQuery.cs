using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;

public sealed record SearchSupplierInvoicesQuery(
    InvoiceStatus? Status,
    Guid? SupplierId)
    : IQuery<IReadOnlyCollection<SupplierInvoiceSummaryResponse>>;
