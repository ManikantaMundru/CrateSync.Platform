using CrateSync.Platform.Warehousing.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Warehousing.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses", "warehousing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => WarehouseId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.DeactivatedAtUtc);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(x => x.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.StateOrProvince)
                .HasColumnName("StateOrProvince")
                .HasMaxLength(100);

            address.Property(x => x.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(30)
                .IsRequired();

            address.Property(x => x.Country)
                .HasColumnName("Country")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Code
        })
        .IsUnique();
    }
}
