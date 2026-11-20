using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.AddOrderLine;

public sealed record AddSalesOrderLineCommand(
    Guid SalesOrderId,
    Guid ProductVarietyId,
    int Quantity,
    decimal PackSize,
    decimal UnitPrice,
    string Currency) : ICommand<Result<Guid>>;
