using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Sales.Infrastructure.Persistence.Configurations;

internal sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines", "sales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SalesOrderLineId.From(value));

        builder.Property<SalesOrderId>("SalesOrderId")
            .HasConversion(id => id.Value, value => SalesOrderId.From(value))
            .IsRequired();

        builder.Property(x => x.ProductVarietyId).IsRequired();
        builder.Property(x => x.BatchCode).HasMaxLength(100);

        builder.Property(x => x.OriginCountry)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(quantity => quantity.Value, value => new CrateQuantity(value))
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(x => x.PackSize)
            .HasConversion(packSize => packSize.Value, value => new PackSize(value))
            .HasColumnName("PackSize")
            .HasPrecision(18, 3)
            .IsRequired();

        builder.OwnsOne(x => x.UnitPrice, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            money.OwnsOne(x => x.Currency, currency =>
            {
                currency.Property(x => x.Code)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.Ignore(x => x.LineTotal);
        builder.HasIndex("SalesOrderId");
        builder.HasIndex(x => new { x.ProductVarietyId, x.OriginCountry });
    }
}
