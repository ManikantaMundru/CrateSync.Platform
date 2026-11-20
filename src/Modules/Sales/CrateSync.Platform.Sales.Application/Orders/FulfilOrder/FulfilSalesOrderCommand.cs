using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.FulfilOrder;

public sealed record FulfilSalesOrderCommand(Guid SalesOrderId) : ICommand<Result>;
