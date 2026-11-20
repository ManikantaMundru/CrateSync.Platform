using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.ConfirmOrder;

public sealed record ConfirmSalesOrderCommand(Guid SalesOrderId) : ICommand<Result>;
