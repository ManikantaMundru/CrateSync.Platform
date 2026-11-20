using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.RemoveOrderLine;

public sealed record RemoveSalesOrderLineCommand(Guid SalesOrderId, Guid SalesOrderLineId)
    : ICommand<Result>;
