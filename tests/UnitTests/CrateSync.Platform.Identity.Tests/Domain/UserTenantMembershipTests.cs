using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Domain.Memberships;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Identity.Tests.Domain;

public sealed class UserTenantMembershipTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Membership_WhenCreated_ShouldBeActive()
    {
        // Arrange / Act
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        // Assert
        membership.IsActive.ShouldBeTrue();
        membership.Roles.ShouldBeEmpty();
    }

    [Fact]
    public void Membership_ShouldSupportMultipleTenantRoles()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        // Act
        membership.AddRole(
            TenantRole.Sales,
            Now.AddMinutes(1));

        membership.AddRole(
            TenantRole.Finance,
            Now.AddMinutes(2));

        // Assert
        membership.Roles
            .Select(x => x.Role)
            .ShouldBe(
                [
                    TenantRole.Sales,
                    TenantRole.Finance
                ],
                ignoreOrder: true);
    }

    [Fact]
    public void Membership_WhenSameRoleAddedTwice_ShouldBeIdempotent()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        // Act
        var first =
            membership.AddRole(
                TenantRole.Warehouse,
                Now.AddMinutes(1));

        var second =
            membership.AddRole(
                TenantRole.Warehouse,
                Now.AddMinutes(2));

        // Assert
        first.ShouldBeTrue();
        second.ShouldBeFalse();

        membership.Roles.Count.ShouldBe(1);
    }

    [Fact]
    public void Membership_WhenDeactivated_ShouldRemainAddressableButNotActive()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        membership.AddRole(
            TenantRole.TenantAdmin,
            Now.AddMinutes(1));

        // Act
        membership.Deactivate(
            Now.AddMinutes(2));

        // Assert
        membership.IsActive.ShouldBeFalse();
        membership.Roles.Single()
            .Role.ShouldBe(TenantRole.TenantAdmin);
    }
}
