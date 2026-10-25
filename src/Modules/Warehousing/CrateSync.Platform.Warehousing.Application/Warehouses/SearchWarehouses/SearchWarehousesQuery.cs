using CrateSync.Platform.BuildingBlocks.Application.Queries;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;

public sealed record SearchWarehousesQuery(
    string? SearchTerm,
    bool? IsActive)
    : IQuery<IReadOnlyCollection<WarehouseSummaryResponse>>;
