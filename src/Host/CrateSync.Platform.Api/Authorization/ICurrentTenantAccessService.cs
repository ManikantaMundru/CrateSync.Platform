namespace CrateSync.Platform.Api.Authorization;

public interface ICurrentTenantAccessService
{
    Task<bool> IsTenantAdminAsync(
        CancellationToken cancellationToken = default);

    Task<bool> HasTenantWideReadAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CanReadWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    Task<bool> CanOperateWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    Task<bool> CanAdminWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> GetAssignedWarehouseIdsAsync(
        CancellationToken cancellationToken = default);
}
