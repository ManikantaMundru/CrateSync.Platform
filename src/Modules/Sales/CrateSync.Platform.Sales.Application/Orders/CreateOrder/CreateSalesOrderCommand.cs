using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.CreateOrder;

public sealed record CreateSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    string OrderNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? Province,
    string? PostalCode,
    string Country) : ICommand<Result<Guid>>;
