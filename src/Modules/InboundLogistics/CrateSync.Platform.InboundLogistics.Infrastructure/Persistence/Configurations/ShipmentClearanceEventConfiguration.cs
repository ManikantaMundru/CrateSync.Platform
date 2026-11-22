using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentClearanceEventConfiguration
    : IEntityTypeConfiguration<ShipmentClearanceEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentClearanceEvent> builder)
    {
        builder.ToTable("ShipmentClearanceEvents", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentClearanceEventId.From(value))
            .ValueGeneratedNever();

        builder.Property<InboundShipmentId>("ShipmentId")
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .IsRequired();

        builder.Property(x => x.Authority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Remarks)
            .HasMaxLength(1000);

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex("ShipmentId");
        builder.HasIndex(x => new { x.Authority, x.OccurredAtUtc });
    }
}
