using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.CancelOrder;

public sealed record CancelSalesOrderCommand(Guid SalesOrderId) : ICommand<Result>;
