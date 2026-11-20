using CrateSync.Platform.Sales.Application.Orders.GetOrder;
using CrateSync.Platform.Sales.Application.Orders.SearchOrders;
using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.Abstractions;

public interface ISalesOrderReadService
{
    Task<SalesOrderResponse?> GetAsync(
        Guid tenantId,
        Guid salesOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SalesOrderSummaryResponse>> SearchAsync(
        Guid tenantId,
        SalesOrderStatus? status,
        Guid? customerId,
        CancellationToken cancellationToken = default);
}
