using CrateSync.Platform.Api.Authorization;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

internal sealed class TestCurrentTenantAccessService
    : ICurrentTenantAccessService
{
    public Task<bool> IsTenantAdminAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> HasTenantWideReadAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> CanReadWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> CanOperateWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> CanAdminWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<IReadOnlySet<Guid>>
        GetAssignedWarehouseIdsAsync(
            CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlySet<Guid>>(
            new HashSet<Guid>());
}
