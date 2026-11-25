using CrateSync.Platform.Identity.Domain.Authorization;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Identity.Tests.Domain;

public sealed class TenantPermissionsTests
{
    [Fact]
    public void Owner_has_all_defined_permissions()
    {
        foreach (var permission in Enum.GetValues<TenantPermission>())
            TenantPermissions.Grants([TenantRole.TenantOwner], permission).ShouldBeTrue();
    }

    [Theory]
    [InlineData(TenantRole.TenantAdmin)]
    [InlineData(TenantRole.Viewer)]
    public void Operational_roles_cannot_access_financial_permissions(TenantRole role)
    {
        foreach (var permission in Enum.GetValues<TenantPermission>())
            if (permission != TenantPermission.ManageOperations)
                TenantPermissions.Grants([role], permission).ShouldBeFalse();
    }

    [Fact]
    public void Finance_cannot_manage_operations_or_roles()
    {
        TenantPermissions.Grants([TenantRole.Finance], TenantPermission.ViewCosts).ShouldBeTrue();
        TenantPermissions.Grants([TenantRole.Finance], TenantPermission.ManageCosts).ShouldBeTrue();
        TenantPermissions.Grants([TenantRole.Finance], TenantPermission.ViewMargins).ShouldBeTrue();
        TenantPermissions.Grants([TenantRole.Finance], TenantPermission.ManageOperations).ShouldBeFalse();
        TenantPermissions.Grants([TenantRole.Finance], TenantPermission.ManageFinancialRoles).ShouldBeFalse();
    }
}
