using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentDeliveryAllocationConfiguration
    : IEntityTypeConfiguration<ShipmentDeliveryAllocation>
{
    public void Configure(EntityTypeBuilder<ShipmentDeliveryAllocation> builder)
    {
        builder.ToTable("ShipmentDeliveryAllocations", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentDeliveryAllocationId.From(value))
            .ValueGeneratedNever();

        builder.Property<ShipmentDeliveryId>("DeliveryId")
            .HasConversion(
                id => id.Value,
                value => ShipmentDeliveryId.From(value))
            .IsRequired();

        builder.Property(x => x.ShipmentLineId)
            .HasConversion(
                id => id.Value,
                value => ShipmentLineId.From(value))
            .IsRequired();

        builder.Property(x => x.Quantity).IsRequired();

        builder.HasIndex("DeliveryId");
        builder.HasIndex(x => x.ShipmentLineId);
    }
}
