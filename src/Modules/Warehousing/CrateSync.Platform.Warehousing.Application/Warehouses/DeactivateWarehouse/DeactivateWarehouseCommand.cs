using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.DeactivateWarehouse;

public sealed record DeactivateWarehouseCommand(Guid WarehouseId) : ICommand<Result>;
