using System.Security.Claims;
using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Infrastructure.Persistence;

namespace CrateSync.Platform.Api.Authorization;

public sealed class CurrentTenantAccessService(
    IHttpContextAccessor httpContextAccessor,
    ITenantContext tenantContext,
    IdentityAccessReader identityAccessReader)
    : ICurrentTenantAccessService
{
    private IdentityAccessSnapshot? _snapshot;
    private bool _loaded;

    public async Task<bool> HasPermissionAsync(
        TenantPermission permission,
        CancellationToken cancellationToken = default)
    {
        if (!tenantContext.HasTenant || !Enum.IsDefined(permission))
            return false;

        if (IsSuperAdmin())
            return true;

        var membership = await GetCurrentMembershipAsync(cancellationToken);
        return membership is not null &&
            TenantPermissions.Grants(membership.Roles, permission);
    }

    public async Task<bool> HasTenantAccessAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsSuperAdmin())
            return tenantContext.HasTenant;

        return await GetCurrentMembershipAsync(
                   cancellationToken) is not null;
    }

    public Task<bool> IsTenantAdminAsync(
        CancellationToken cancellationToken = default) =>
        HasPermissionAsync(TenantPermission.ManageOperations, cancellationToken);

    public async Task<bool> HasTenantWideReadAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsSuperAdmin())
            return true;

        var membership =
            await GetCurrentMembershipAsync(
                cancellationToken);

        if (membership is null)
            return false;

        return membership.Roles.Contains(
                   TenantRole.TenantAdmin) ||
               membership.Roles.Contains(
                   TenantRole.Viewer) ||
               membership.Roles.Any(TenantPermissions.IsFinancialRole);
    }

    public async Task<bool> CanReadWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        if (await HasTenantWideReadAsync(
                cancellationToken))
        {
            return true;
        }

        var membership =
            await GetCurrentMembershipAsync(
                cancellationToken);

        return membership?.Warehouses.Any(
            x => x.WarehouseId == warehouseId) == true;
    }

    public async Task<bool> CanOperateWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        if (IsSuperAdmin() ||
            await IsTenantAdminAsync(
                cancellationToken))
        {
            return true;
        }

        var membership =
            await GetCurrentMembershipAsync(
                cancellationToken);

        return membership is not null &&
            !membership.Roles.Contains(TenantRole.Finance) &&
            membership.Warehouses.Any(
            x =>
                x.WarehouseId == warehouseId &&
                x.Role is
                    WarehouseAccessRole.WarehouseUser
                    or WarehouseAccessRole.WarehouseAdmin);
    }

    public async Task<bool> CanAdminWarehouseAsync(
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        if (IsSuperAdmin() ||
            await IsTenantAdminAsync(
                cancellationToken))
        {
            return true;
        }

        var membership =
            await GetCurrentMembershipAsync(
                cancellationToken);

        return membership is not null &&
            !membership.Roles.Contains(TenantRole.Finance) &&
            membership.Warehouses.Any(
            x =>
                x.WarehouseId == warehouseId &&
                x.Role ==
                    WarehouseAccessRole.WarehouseAdmin);
    }

    public async Task<IReadOnlySet<Guid>>
        GetAssignedWarehouseIdsAsync(
            CancellationToken cancellationToken = default)
    {
        var membership =
            await GetCurrentMembershipAsync(
                cancellationToken);

        return membership?.Warehouses
                   .Select(x => x.WarehouseId)
                   .ToHashSet()
               ?? new HashSet<Guid>();
    }

    private async Task<TenantAccessSnapshot?>
        GetCurrentMembershipAsync(
            CancellationToken cancellationToken)
    {
        if (!tenantContext.HasTenant)
            return null;

        var snapshot =
            await GetSnapshotAsync(
                cancellationToken);

        return snapshot?.Tenants
            .SingleOrDefault(
                x =>
                    x.TenantId ==
                    tenantContext.TenantId);
    }

    private async Task<IdentityAccessSnapshot?>
        GetSnapshotAsync(
            CancellationToken cancellationToken)
    {
        if (_loaded)
            return _snapshot;

        _loaded = true;

        var userId = GetUserId();

        if (!userId.HasValue)
            return null;

        _snapshot =
            await identityAccessReader.GetAsync(
                userId.Value,
                cancellationToken);

        return _snapshot;
    }

    private Guid? GetUserId()
    {
        var user =
            httpContextAccessor.HttpContext?.User;

        var subject =
            user?.FindFirstValue(
                CrateSyncClaimTypes.Subject)
            ?? user?.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            subject,
            out var userId)
            ? userId
            : null;
    }

    private bool IsSuperAdmin()
    {
        return httpContextAccessor
            .HttpContext?
            .User
            .HasClaim(
                CrateSyncClaimTypes.PlatformRole,
                "SuperAdmin") == true;
    }
}
