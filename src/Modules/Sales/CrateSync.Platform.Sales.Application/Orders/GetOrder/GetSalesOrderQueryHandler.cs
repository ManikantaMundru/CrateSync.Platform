using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Application.Orders.Common;

namespace CrateSync.Platform.Sales.Application.Orders.GetOrder;

internal sealed class GetSalesOrderQueryHandler(
    ISalesOrderReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<GetSalesOrderQuery, Result<SalesOrderResponse>>
{
    public async Task<Result<SalesOrderResponse>> Handle(
        GetSalesOrderQuery request,
        CancellationToken cancellationToken)
    {
        var order = await readService.GetAsync(
            tenantContext.TenantId,
            request.SalesOrderId,
            cancellationToken);

        return order is null
            ? Result<SalesOrderResponse>.Failure(SalesErrors.OrderNotFound)
            : Result<SalesOrderResponse>.Success(order);
    }
}
