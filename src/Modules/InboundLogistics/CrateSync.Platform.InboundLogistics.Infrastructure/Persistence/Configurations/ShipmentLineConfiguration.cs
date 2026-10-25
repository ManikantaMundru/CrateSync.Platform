using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using CrateSync.Platform.InboundLogistics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("InboundShipmentLines", "logistics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ShipmentLineId.From(value))
            .ValueGeneratedNever();

        builder.Property<InboundShipmentId>("ShipmentId")
            .HasConversion(
                id => id.Value,
                value => InboundShipmentId.From(value))
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.ProductVarietyId)
            .IsRequired();

        builder.Property(x => x.BatchCode)
            .HasConversion(
                batchCode => batchCode.Value,
                value => new BatchCode(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ExpectedQuantity)
            .HasConversion(
                quantity => quantity.Value,
                value => new CrateQuantity(value))
            .IsRequired();

        builder.Property(x => x.ReceivedQuantity)
            .HasConversion(
                quantity => quantity.Value,
                value => new CrateQuantity(value))
            .IsRequired();

        builder.Property(x => x.PackSize)
            .HasConversion(
                packSize => packSize.WeightKg,
                value => new PackSize(value))
            .HasColumnType("decimal(18,3)")
            .IsRequired();

        builder.OwnsOne(x => x.UnitCost, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("UnitCost")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.OwnsOne(x => x.Currency, currency =>
            {
                currency.Property(x => x.Code)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.HasIndex("ShipmentId");

        builder.HasIndex(x => x.ProductVarietyId);

        builder.HasIndex(x => x.BatchCode);
    }
}
