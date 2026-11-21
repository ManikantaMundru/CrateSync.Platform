using CrateSync.Platform.Identity.Domain.Memberships;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserWarehouseAssignmentConfiguration
    : IEntityTypeConfiguration<UserWarehouseAssignment>
{
    public void Configure(
        EntityTypeBuilder<UserWarehouseAssignment> builder)
    {
        builder.ToTable(
            "UserWarehouseAssignments",
            "identity");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MembershipId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(
                x => new
                {
                    x.MembershipId,
                    x.WarehouseId
                })
            .IsUnique();

        builder.HasIndex(x => x.WarehouseId);
    }
}
