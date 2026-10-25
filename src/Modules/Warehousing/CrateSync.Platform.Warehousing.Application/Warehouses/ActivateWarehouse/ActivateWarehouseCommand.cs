using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.ActivateWarehouse;

public sealed record ActivateWarehouseCommand(Guid WarehouseId) : ICommand<Result>;
