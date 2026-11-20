using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence.Configurations;

internal sealed class SupplierInvoiceLineConfiguration
    : IEntityTypeConfiguration<SupplierInvoiceLine>
{
    public void Configure(
        EntityTypeBuilder<SupplierInvoiceLine> builder)
    {
        builder.ToTable("SupplierInvoiceLines", "finance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => SupplierInvoiceLineId.From(value));

        builder.Property<SupplierInvoiceId>("SupplierInvoiceId")
            .HasConversion(
                id => id.Value,
                value => SupplierInvoiceId.From(value))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.ProductVarietyId);
        builder.Property(x => x.Quantity).IsRequired();

        builder.OwnsOne(x => x.UnitCost, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("UnitCost")
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

        builder.HasIndex("SupplierInvoiceId");
        builder.HasIndex(x => x.ProductVarietyId);
    }
}
