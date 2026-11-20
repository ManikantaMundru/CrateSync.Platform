using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Sales.Application.Orders.Common;

internal static class SalesErrors
{
    public static readonly Error OrderNotFound = new(
        "Sales.OrderNotFound",
        "Sales order was not found.",
        ErrorType.NotFound);

    public static Error Conflict(string description) => new(
        "Sales.InvalidOrderState",
        description,
        ErrorType.Conflict);
}
