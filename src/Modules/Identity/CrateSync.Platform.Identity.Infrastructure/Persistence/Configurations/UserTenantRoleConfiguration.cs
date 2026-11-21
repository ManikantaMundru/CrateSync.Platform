using CrateSync.Platform.Identity.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserTenantRoleConfiguration
    : IEntityTypeConfiguration<UserTenantRole>
{
    public void Configure(
        EntityTypeBuilder<UserTenantRole> builder)
    {
        builder.ToTable(
            "UserTenantRoles",
            "identity");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MembershipId)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.MembershipId,
                    x.Role
                })
            .IsUnique();
    }
}
