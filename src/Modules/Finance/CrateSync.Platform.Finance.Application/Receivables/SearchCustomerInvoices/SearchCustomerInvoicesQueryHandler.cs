using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Application.Abstractions;

namespace CrateSync.Platform.Finance.Application.Receivables.SearchCustomerInvoices;

internal sealed class SearchCustomerInvoicesQueryHandler(
    IFinanceReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<SearchCustomerInvoicesQuery, IReadOnlyCollection<CustomerInvoiceSummaryResponse>>
{
    public Task<IReadOnlyCollection<CustomerInvoiceSummaryResponse>> Handle(
        SearchCustomerInvoicesQuery request,
        CancellationToken cancellationToken)
    {
        return readService.SearchCustomerInvoicesAsync(
            tenantContext.TenantId,
            request.Status,
            request.CustomerId,
            cancellationToken);
    }
}
