using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Sales.Application.Orders.GetOrder;

public sealed record GetSalesOrderQuery(Guid SalesOrderId)
    : IQuery<Result<SalesOrderResponse>>;
