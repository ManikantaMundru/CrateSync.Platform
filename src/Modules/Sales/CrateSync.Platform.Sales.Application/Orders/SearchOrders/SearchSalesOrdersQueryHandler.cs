using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Sales.Application.Abstractions;

namespace CrateSync.Platform.Sales.Application.Orders.SearchOrders;

internal sealed class SearchSalesOrdersQueryHandler(
    ISalesOrderReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<SearchSalesOrdersQuery, IReadOnlyCollection<SalesOrderSummaryResponse>>
{
    public Task<IReadOnlyCollection<SalesOrderSummaryResponse>> Handle(
        SearchSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return readService.SearchAsync(
            tenantContext.TenantId,
            request.Status,
            request.CustomerId,
            cancellationToken);
    }
}
