using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.RequestDispatchOrder;

public sealed record RequestDispatchSalesOrderCommand(Guid SalesOrderId) : ICommand<Result>;
