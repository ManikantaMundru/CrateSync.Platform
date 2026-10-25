using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.UpdateWarehouse;

public sealed record UpdateWarehouseCommand(
    Guid WarehouseId,
    string Name,
    string Code,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string? StateOrProvince,
    string PostalCode,
    string Country)
    : ICommand<Result>;
