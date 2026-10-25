using CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;
using CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;

namespace CrateSync.Platform.Warehousing.Application.Abstractions;

public interface IWarehousingReadService
{
    Task<WarehouseResponse?> GetByIdAsync(Guid tenantId, Guid warehouseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WarehouseSummaryResponse>> SearchAsync(
        Guid tenantId,
        string? searchTerm,
        bool? isActive,
        CancellationToken cancellationToken = default);
}
