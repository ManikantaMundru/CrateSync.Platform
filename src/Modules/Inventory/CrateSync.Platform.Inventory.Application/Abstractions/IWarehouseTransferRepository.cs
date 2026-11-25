using CrateSync.Platform.Inventory.Domain.Stock;
namespace CrateSync.Platform.Inventory.Application.Abstractions;
public interface IWarehouseTransferRepository
{
    Task<WarehouseTransfer?> GetAsync(Guid tenantId, Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<WarehouseTransfer>> ListAsync(Guid tenantId, CancellationToken ct);
    void Add(WarehouseTransfer transfer);
}
