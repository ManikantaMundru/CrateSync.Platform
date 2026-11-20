using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.AllocateOrder;

public sealed record AllocateSalesOrderCommand(Guid SalesOrderId) : ICommand<Result>;
