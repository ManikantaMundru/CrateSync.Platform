using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class InboundShipmentConfiguration : IEntityTypeConfiguration<InboundShipment>
{
    public void Configure(EntityTypeBuilder<InboundShipment> builder)
    {
        builder.ToTable("InboundShipments", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.SupplierId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.TrackingNumber)
            .HasConversion(
                trackingNumber => trackingNumber.Value,
                value => new TrackingNumber(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.OriginCountry)
            .HasConversion(
                country => country.Value,
                value => new CountryCode(value))
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ExpectedArrivalAtUtc)
            .IsRequired();

        builder.Property(x => x.ShippedAtUtc);

        builder.Property(x => x.ArrivedAtUtc);

        builder.Property(x => x.ReceivedAtUtc);

        builder.Property(x => x.CancelledAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.TrackingNumber })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.Status });

        builder.HasIndex(x => new { x.TenantId, x.SupplierId });

        builder.HasIndex(x => new { x.TenantId, x.WarehouseId });

        builder.HasIndex(x => new { x.TenantId, x.ExpectedArrivalAtUtc });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("ShipmentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
