using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;

public sealed record GetWarehouseQuery(Guid WarehouseId) : IQuery<Result<WarehouseResponse>>;
