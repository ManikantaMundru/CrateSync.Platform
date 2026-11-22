using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentReceiptConfiguration
    : IEntityTypeConfiguration<ShipmentReceipt>
{
    public void Configure(EntityTypeBuilder<ShipmentReceipt> builder)
    {
        builder.ToTable("ShipmentReceipts", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentReceiptId.From(value))
            .ValueGeneratedNever();

        builder.Property<InboundShipmentId>("ShipmentId")
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .IsRequired();

        builder.Property(x => x.ShipmentLineId)
            .HasConversion(
                id => id.Value,
                value => ShipmentLineId.From(value))
            .IsRequired();

        builder.Property(x => x.ProductVarietyId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.ReceivedAtUtc)
            .IsRequired();

        builder.HasIndex("ShipmentId");
        builder.HasIndex(x => x.ShipmentLineId);
    }
}
