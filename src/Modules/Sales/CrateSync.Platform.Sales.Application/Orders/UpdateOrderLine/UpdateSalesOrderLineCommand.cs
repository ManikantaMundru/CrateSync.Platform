using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.UpdateOrderLine;

public sealed record UpdateSalesOrderLineCommand(
    Guid SalesOrderId,
    Guid SalesOrderLineId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency) : ICommand<Result>;
