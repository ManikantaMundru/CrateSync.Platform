using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.Common;

internal static class InventoryErrors
{
    public static readonly Error StockNotFound = new(
        "Inventory.StockNotFound",
        "Inventory balance was not found.",
        ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}
