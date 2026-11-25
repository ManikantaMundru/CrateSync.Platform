using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;
internal sealed class RegulatorySampleConfiguration : IEntityTypeConfiguration<RegulatorySample>
{
    public void Configure(EntityTypeBuilder<RegulatorySample> builder)
    {
        builder.ToTable("RegulatorySamples", "logistics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property<InboundShipmentId>("ShipmentId").HasConversion(x => x.Value, x => InboundShipmentId.From(x)).IsRequired();
        builder.Property(x => x.Authority).HasConversion<int>();
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Remarks).HasMaxLength(500).IsRequired();
        builder.HasIndex("ShipmentId");
        builder.HasIndex(x => x.ShipmentLineId);
    }
}
