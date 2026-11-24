using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentDeliveryConfiguration
    : IEntityTypeConfiguration<ShipmentDelivery>
{
    public void Configure(EntityTypeBuilder<ShipmentDelivery> builder)
    {
        builder.ToTable("ShipmentDeliveries", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentDeliveryId.From(value))
            .ValueGeneratedNever();

        builder.Property<InboundShipmentId>("ShipmentId")
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .IsRequired();

        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.TransportCompany).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DriverName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DriverPhone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.VehicleNumber).HasMaxLength(50);
        builder.Property(x => x.OriginLocation).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DestinationLocation).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TransportReference).HasMaxLength(100);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex("ShipmentId");
        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey("DeliveryId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
