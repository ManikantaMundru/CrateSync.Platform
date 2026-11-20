using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.Orders.SearchOrders;

public sealed record SearchSalesOrdersQuery(
    SalesOrderStatus? Status,
    Guid? CustomerId) : IQuery<IReadOnlyCollection<SalesOrderSummaryResponse>>;
