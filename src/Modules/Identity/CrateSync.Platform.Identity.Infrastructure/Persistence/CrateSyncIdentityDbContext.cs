using CrateSync.Platform.Identity.Domain.Memberships;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence;

public sealed class CrateSyncIdentityDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public CrateSyncIdentityDbContext(
        DbContextOptions<CrateSyncIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserTenantMembership> UserTenantMemberships =>
        Set<UserTenantMembership>();

    public DbSet<UserTenantRole> UserTenantRoles =>
        Set<UserTenantRole>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        ConfigureIdentityTables(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(CrateSyncIdentityDbContext).Assembly);
    }

    private static void ConfigureIdentityTables(
        ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>()
            .ToTable("Users", "identity");

        builder.Entity<IdentityRole<Guid>>()
            .ToTable("Roles", "identity");

        builder.Entity<IdentityUserRole<Guid>>()
            .ToTable("UserRoles", "identity");

        builder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("UserClaims", "identity");

        builder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("UserLogins", "identity");

        builder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("RoleClaims", "identity");

        builder.Entity<IdentityUserToken<Guid>>()
            .ToTable("UserTokens", "identity");
    }
}
