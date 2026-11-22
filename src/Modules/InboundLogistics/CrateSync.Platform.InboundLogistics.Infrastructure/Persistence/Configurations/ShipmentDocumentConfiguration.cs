using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentDocumentConfiguration
    : IEntityTypeConfiguration<ShipmentDocument>
{
    public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
    {
        builder.ToTable("ShipmentDocuments", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentDocumentId.From(value))
            .ValueGeneratedNever();

        builder.Property<InboundShipmentId>("ShipmentId")
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.DocumentName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex("ShipmentId");
        builder.HasIndex(x => x.Type);
    }
}
