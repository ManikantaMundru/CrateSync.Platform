using CrateSync.Platform.Identity.Domain.Memberships;
using CrateSync.Platform.Identity.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserTenantMembershipConfiguration
    : IEntityTypeConfiguration<UserTenantMembership>
{
    public void Configure(
        EntityTypeBuilder<UserTenantMembership> builder)
    {
        builder.ToTable(
            "UserTenantMemberships",
            "identity");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.UserId,
                    x.TenantId
                })
            .IsUnique();

        builder.HasIndex(
            x => new
            {
                x.TenantId,
                x.IsActive
            });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Roles)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}
