using CrateSync.Platform.Warehousing.Domain.Warehouses;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.common;

public static class WarehouseErrors
{
    public static Error NotFound(WarehouseId warehouseId) =>
        Error.NotFound("Warehousing.Warehouse.NotFound", $"Warehouse '{warehouseId.Value}' was not found.");

    public static Error DuplicateCode(string code) =>
        Error.Conflict("Warehousing.Warehouse.DuplicateCode", $"A warehouse with code '{code}' already exists.");
}
