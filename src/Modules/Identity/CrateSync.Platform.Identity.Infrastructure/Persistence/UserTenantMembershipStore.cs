using CrateSync.Platform.Identity.Domain.Memberships;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence;

public sealed class UserTenantMembershipStore(
    CrateSyncIdentityDbContext dbContext)
{
    public Task<List<UserTenantMembership>>
        GetActiveMembershipsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        return dbContext.UserTenantMemberships
            .Include(x => x.Roles)
            .Include(x => x.WarehouseAssignments)
            .Where(
                x =>
                    x.UserId == userId &&
                    x.IsActive)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<UserTenantMembership?>
        GetActiveMembershipAsync(
            Guid userId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
    {
        return dbContext.UserTenantMemberships
            .Include(x => x.Roles)
            .Include(x => x.WarehouseAssignments)
            .SingleOrDefaultAsync(
                x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId &&
                    x.IsActive,
                cancellationToken);
    }
}
