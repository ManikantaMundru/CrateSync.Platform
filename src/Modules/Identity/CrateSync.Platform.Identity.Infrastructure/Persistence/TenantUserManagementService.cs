using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Domain.Memberships;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence;

public sealed record TenantUserSummary(
    Guid UserId,
    string UserName,
    string? Email,
    string FirstName,
    string LastName,
    bool IsMembershipActive,
    bool RequiresActivation,
    IReadOnlyCollection<TenantRole> Roles,
    IReadOnlyCollection<WarehouseAccessSnapshot> Warehouses);

public sealed record TenantUserProvisionResult(
    bool Succeeded,
    Guid? UserId,
    bool CreatedNewIdentity,
    bool RequiresActivation,
    IReadOnlyCollection<string> Errors)
{
    public static TenantUserProvisionResult Success(
        Guid userId,
        bool createdNewIdentity,
        bool requiresActivation) =>
        new(
            true,
            userId,
            createdNewIdentity,
            requiresActivation,
            Array.Empty<string>());

    public static TenantUserProvisionResult Failure(
        params string[] errors) =>
        new(
            false,
            null,
            false,
            false,
            errors);
}

public sealed record WarehouseAccessInput(
    Guid WarehouseId,
    WarehouseAccessRole Role);

public sealed class TenantUserManagementService(
    CrateSyncIdentityDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    TimeProvider timeProvider)
{
    public async Task<IReadOnlyCollection<TenantUserSummary>>
        GetUsersAsync(
            Guid tenantId,
            CancellationToken cancellationToken = default)
    {
        var memberships =
            await dbContext.UserTenantMemberships
                .AsNoTracking()
                .Include(x => x.Roles)
                .Include(x => x.WarehouseAssignments)
                .Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
            return Array.Empty<TenantUserSummary>();

        var userIds =
            memberships
                .Select(x => x.UserId)
                .Distinct()
                .ToArray();

        var users =
            await dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    cancellationToken);

        return memberships
            .Where(x => users.ContainsKey(x.UserId))
            .Select(x =>
            {
                var user = users[x.UserId];

                return new TenantUserSummary(
                    user.Id,
                    user.UserName
                        ?? user.Email
                        ?? user.Id.ToString(),
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    x.IsActive,
                    !user.EmailConfirmed ||
                    string.IsNullOrWhiteSpace(
                        user.PasswordHash),
                    x.Roles
                        .Select(role => role.Role)
                        .OrderBy(role => role)
                        .ToArray(),
                    x.WarehouseAssignments
                        .Select(warehouse =>
                            new WarehouseAccessSnapshot(
                                warehouse.WarehouseId,
                                warehouse.Role))
                        .OrderBy(warehouse =>
                            warehouse.WarehouseId)
                        .ToArray());
            })
            .OrderBy(x => x.UserName)
            .ToArray();
    }

    public async Task<TenantUserProvisionResult>
        CreateOrAddAsync(
            Guid tenantId,
            string email,
            string? userName,
            string firstName,
            string lastName,
            IReadOnlyCollection<TenantRole> roles,
            IReadOnlyCollection<WarehouseAccessInput> warehouses,
            CancellationToken cancellationToken = default)
    {
        email = email.Trim();
        userName =
            string.IsNullOrWhiteSpace(userName)
                ? email
                : userName.Trim();

        var now = timeProvider.GetUtcNow();

        var user =
            await userManager.FindByEmailAsync(email);

        var createdNewIdentity = false;

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = email,
                EmailConfirmed = false,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                IsActive = true,
                PlatformRole = PlatformRole.None,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            var createResult =
                await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return TenantUserProvisionResult.Failure(
                    createResult.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }

            createdNewIdentity = true;
        }
        else if (!user.IsActive)
        {
            return TenantUserProvisionResult.Failure(
                "The existing identity is disabled at platform level.");
        }
        else if (user.PlatformRole != PlatformRole.None)
        {
            return TenantUserProvisionResult.Failure(
                "Platform administrator identities can only be assigned through platform administration.");
        }

        var membership =
            await dbContext.UserTenantMemberships
                .Include(x => x.Roles)
                .Include(x => x.WarehouseAssignments)
                .SingleOrDefaultAsync(
                    x =>
                        x.UserId == user.Id &&
                        x.TenantId == tenantId,
                    cancellationToken);

        if (membership is null)
        {
            membership =
                UserTenantMembership.Create(
                    user.Id,
                    tenantId,
                    now);

            dbContext.UserTenantMemberships.Add(
                membership);

            ReplaceAccess(
                membership,
                roles,
                warehouses,
                now);
        }
        else
        {
            membership.Activate(now);

            await ReconcilePersistedAccessAsync(
                membership,
                roles,
                warehouses,
                now,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        var requiresActivation =
            !user.EmailConfirmed ||
            !await userManager.HasPasswordAsync(user);

        return TenantUserProvisionResult.Success(
            user.Id,
            createdNewIdentity,
            requiresActivation);
    }

    public async Task<bool> CanManageUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user =
            await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (user is null ||
            user.PlatformRole != PlatformRole.None)
        {
            return false;
        }

        return await dbContext.UserTenantMemberships
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.UserId == userId &&
                    x.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<bool> UpdateAccessAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<TenantRole> roles,
        IReadOnlyCollection<WarehouseAccessInput> warehouses,
        CancellationToken cancellationToken = default)
    {
        var user =
            await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (user is null ||
            user.PlatformRole != PlatformRole.None)
        {
            return false;
        }

        var membership =
            await dbContext.UserTenantMemberships
                .SingleOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.TenantId == tenantId,
                    cancellationToken);

        if (membership is null)
            return false;

        await ReconcilePersistedAccessAsync(
            membership,
            roles,
            warehouses,
            timeProvider.GetUtcNow(),
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> SetMembershipActiveAsync(
        Guid tenantId,
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user =
            await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (user is null ||
            user.PlatformRole != PlatformRole.None)
        {
            return false;
        }

        var membership =
            await dbContext.UserTenantMemberships
                .SingleOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.TenantId == tenantId,
                    cancellationToken);

        if (membership is null)
            return false;

        var now = timeProvider.GetUtcNow();

        if (isActive)
            membership.Activate(now);
        else
            membership.Deactivate(now);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task ReconcilePersistedAccessAsync(
        UserTenantMembership membership,
        IReadOnlyCollection<TenantRole> roles,
        IReadOnlyCollection<WarehouseAccessInput> warehouses,
        DateTimeOffset occurredAtUtc,
        CancellationToken cancellationToken)
    {
        var desiredRoles =
            roles
                .Distinct()
                .ToHashSet();

        var existingRoles =
            await dbContext.UserTenantRoles
                .Where(
                    x =>
                        x.MembershipId ==
                        membership.Id)
                .ToListAsync(cancellationToken);

        var rolesToRemove =
            existingRoles
                .Where(
                    x =>
                        !desiredRoles.Contains(
                            x.Role))
                .ToArray();

        if (rolesToRemove.Length > 0)
        {
            dbContext.UserTenantRoles
                .RemoveRange(rolesToRemove);
        }

        var existingRoleValues =
            existingRoles
                .Select(x => x.Role)
                .ToHashSet();

        foreach (var role in desiredRoles)
        {
            if (existingRoleValues.Contains(role))
                continue;

            dbContext.UserTenantRoles.Add(
                UserTenantRole.Create(
                    membership.Id,
                    role,
                    occurredAtUtc));
        }

        var desiredWarehouses =
            warehouses
                .GroupBy(x => x.WarehouseId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Last().Role);

        var existingWarehouses =
            await dbContext.UserWarehouseAssignments
                .Where(
                    x =>
                        x.MembershipId ==
                        membership.Id)
                .ToListAsync(cancellationToken);

        var warehousesToRemove =
            existingWarehouses
                .Where(
                    x =>
                        !desiredWarehouses.ContainsKey(
                            x.WarehouseId))
                .ToArray();

        if (warehousesToRemove.Length > 0)
        {
            dbContext.UserWarehouseAssignments
                .RemoveRange(
                    warehousesToRemove);
        }

        var existingWarehousesById =
            existingWarehouses
                .ToDictionary(
                    x => x.WarehouseId);

        foreach (var warehouse in desiredWarehouses)
        {
            if (existingWarehousesById.TryGetValue(
                    warehouse.Key,
                    out var existing))
            {
                if (existing.Role != warehouse.Value)
                {
                    existing.ChangeRole(
                        warehouse.Value,
                        occurredAtUtc);
                }

                continue;
            }

            dbContext.UserWarehouseAssignments.Add(
                UserWarehouseAssignment.Create(
                    membership.Id,
                    warehouse.Key,
                    warehouse.Value,
                    occurredAtUtc));
        }

        dbContext.Entry(membership)
            .Property(x => x.UpdatedAtUtc)
            .CurrentValue = occurredAtUtc;
    }

    private static void ReplaceAccess(
        UserTenantMembership membership,
        IReadOnlyCollection<TenantRole> roles,
        IReadOnlyCollection<WarehouseAccessInput> warehouses,
        DateTimeOffset occurredAtUtc)
    {
        var desiredRoles =
            roles
                .Distinct()
                .ToHashSet();

        foreach (var role in
                 membership.Roles
                     .Select(x => x.Role)
                     .ToArray())
        {
            if (!desiredRoles.Contains(role))
                membership.RemoveRole(
                    role,
                    occurredAtUtc);
        }

        foreach (var role in desiredRoles)
        {
            membership.AddRole(
                role,
                occurredAtUtc);
        }

        var desiredWarehouses =
            warehouses
                .GroupBy(x => x.WarehouseId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Last().Role);

        foreach (var warehouseId in
                 membership.WarehouseAssignments
                     .Select(x => x.WarehouseId)
                     .ToArray())
        {
            if (!desiredWarehouses.ContainsKey(
                    warehouseId))
            {
                membership.RemoveWarehouse(
                    warehouseId,
                    occurredAtUtc);
            }
        }

        foreach (var warehouse in desiredWarehouses)
        {
            membership.AssignWarehouse(
                warehouse.Key,
                warehouse.Value,
                occurredAtUtc);
        }
    }
}
