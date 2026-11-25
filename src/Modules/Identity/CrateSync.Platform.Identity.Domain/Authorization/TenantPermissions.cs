namespace CrateSync.Platform.Identity.Domain.Authorization;

public enum TenantPermission
{
    ManageOperations,
    ManageFinancialRoles,
    ViewCosts,
    ManageCosts,
    ViewMargins,
    ManageFinance,
    ApprovePricing
}

public static class TenantPermissions
{
    public static bool IsFinancialRole(TenantRole role) =>
        role is TenantRole.TenantOwner or TenantRole.Finance;

    public static bool Grants(
        IEnumerable<TenantRole> roles,
        TenantPermission permission)
    {
        var assigned = roles.ToHashSet();
        if (assigned.Contains(TenantRole.TenantOwner))
            return Enum.IsDefined(permission);

        // Finance is deliberately read-only for operations.
        if (assigned.Contains(TenantRole.Finance))
            return permission is TenantPermission.ViewCosts
                or TenantPermission.ManageCosts
                or TenantPermission.ViewMargins
                or TenantPermission.ManageFinance;

        return permission == TenantPermission.ManageOperations
            && assigned.Contains(TenantRole.TenantAdmin);
    }
}
