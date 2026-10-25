namespace CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;

public sealed record WarehouseSummaryResponse(
    Guid Id,
    string Name,
    string Code,
    string City,
    string Country,
    bool IsActive);
