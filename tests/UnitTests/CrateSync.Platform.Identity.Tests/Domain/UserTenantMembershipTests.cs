using CrateSync.Platform.Identity.Domain.Authorization;
using CrateSync.Platform.Identity.Domain.Memberships;
using Shouldly;
using Xunit;

namespace CrateSync.Platform.Identity.Tests.Domain;

public sealed class UserTenantMembershipTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

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
        membership.WarehouseAssignments.ShouldBeEmpty();
    }

    [Fact]
    public void Membership_ShouldSupportTenantAdminRole()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        // Act
        var added =
            membership.AddRole(
                TenantRole.TenantAdmin,
                Now.AddMinutes(1));

        // Assert
        added.ShouldBeTrue();
        membership.Roles.Single()
            .Role.ShouldBe(TenantRole.TenantAdmin);
    }

    [Fact]
    public void Membership_WhenSameTenantRoleAddedTwice_ShouldBeIdempotent()
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
                TenantRole.Viewer,
                Now.AddMinutes(1));

        var second =
            membership.AddRole(
                TenantRole.Viewer,
                Now.AddMinutes(2));

        // Assert
        first.ShouldBeTrue();
        second.ShouldBeFalse();
        membership.Roles.Count.ShouldBe(1);
    }

    [Fact]
    public void AssignWarehouse_ShouldScopeRoleToWarehouse()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        var warehouseId = Guid.NewGuid();

        // Act
        var assigned =
            membership.AssignWarehouse(
                warehouseId,
                WarehouseAccessRole.WarehouseAdmin,
                Now.AddMinutes(1));

        // Assert
        assigned.ShouldBeTrue();

        var assignment =
            membership.WarehouseAssignments.Single();

        assignment.WarehouseId.ShouldBe(warehouseId);
        assignment.Role.ShouldBe(
            WarehouseAccessRole.WarehouseAdmin);
    }

    [Fact]
    public void AssignWarehouse_WhenRoleChanges_ShouldUpdateExistingAssignment()
    {
        // Arrange
        var membership =
            UserTenantMembership.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Now);

        var warehouseId = Guid.NewGuid();

        membership.AssignWarehouse(
            warehouseId,
            WarehouseAccessRole.WarehouseUser,
            Now.AddMinutes(1));

        // Act
        var changed =
            membership.AssignWarehouse(
                warehouseId,
                WarehouseAccessRole.WarehouseAdmin,
                Now.AddMinutes(2));

        // Assert
        changed.ShouldBeTrue();
        membership.WarehouseAssignments.Count.ShouldBe(1);
        membership.WarehouseAssignments.Single()
            .Role.ShouldBe(
                WarehouseAccessRole.WarehouseAdmin);
    }

    [Fact]
    public void Membership_WhenDeactivated_ShouldKeepAccessConfiguration()
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

        membership.AssignWarehouse(
            Guid.NewGuid(),
            WarehouseAccessRole.WarehouseAdmin,
            Now.AddMinutes(2));

        // Act
        membership.Deactivate(
            Now.AddMinutes(3));

        // Assert
        membership.IsActive.ShouldBeFalse();
        membership.Roles.ShouldNotBeEmpty();
        membership.WarehouseAssignments.ShouldNotBeEmpty();
    }
}
