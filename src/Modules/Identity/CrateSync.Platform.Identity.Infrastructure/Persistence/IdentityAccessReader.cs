using CrateSync.Platform.Identity.Domain.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence;

public sealed record IdentityAccessSnapshot(
    Guid UserId,
    string DisplayName,
    string? Email,
    PlatformRole PlatformRole,
    IReadOnlyCollection<TenantAccessSnapshot> Tenants);

public sealed record TenantAccessSnapshot(
    Guid TenantId,
    IReadOnlyCollection<TenantRole> Roles,
    IReadOnlyCollection<WarehouseAccessSnapshot> Warehouses);

public sealed record WarehouseAccessSnapshot(
    Guid WarehouseId,
    WarehouseAccessRole Role);

public sealed class IdentityAccessReader(
    CrateSyncIdentityDbContext dbContext)
{
    public async Task<IdentityAccessSnapshot?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == userId && x.IsActive,
                cancellationToken);

        if (user is null)
            return null;

        var memberships = await dbContext.UserTenantMemberships
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.WarehouseAssignments)
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var displayName =
            $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = user.UserName ?? user.Email ?? user.Id.ToString();

        return new IdentityAccessSnapshot(
            user.Id,
            displayName,
            user.Email,
            user.PlatformRole,
            memberships
                .Select(x =>
                    new TenantAccessSnapshot(
                        x.TenantId,
                        x.Roles
                            .Select(role => role.Role)
                            .OrderBy(role => role)
                            .ToArray(),
                        x.WarehouseAssignments
                            .Select(warehouse =>
                                new WarehouseAccessSnapshot(
                                    warehouse.WarehouseId,
                                    warehouse.Role))
                            .OrderBy(warehouse => warehouse.WarehouseId)
                            .ToArray()))
                .ToArray());
    }
}
