using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Application.Abstractions;

namespace CrateSync.Platform.Finance.Application.Payables.SearchSupplierInvoices;

internal sealed class SearchSupplierInvoicesQueryHandler(
    IFinanceReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<SearchSupplierInvoicesQuery, IReadOnlyCollection<SupplierInvoiceSummaryResponse>>
{
    public Task<IReadOnlyCollection<SupplierInvoiceSummaryResponse>> Handle(
        SearchSupplierInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        return readService.SearchSupplierInvoicesAsync(
            tenantContext.TenantId,
            request.Status,
            request.SupplierId,
            cancellationToken);
    }
}
