using CrateSync.Platform.Warehousing.Domain.Warehouses;

namespace CrateSync.Platform.Warehousing.Application.Abstractions;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(WarehouseId warehouseId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);

    void Add(Warehouse warehouse);
}
