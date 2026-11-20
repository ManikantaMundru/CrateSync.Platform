using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence.Configurations;

internal sealed class CustomerInvoiceLineConfiguration
    : IEntityTypeConfiguration<CustomerInvoiceLine>
{
    public void Configure(
        EntityTypeBuilder<CustomerInvoiceLine> builder)
    {
        builder.ToTable("CustomerInvoiceLines", "finance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerInvoiceLineId.From(value));

        builder.Property<CustomerInvoiceId>("CustomerInvoiceId")
            .HasConversion(
                id => id.Value,
                value => CustomerInvoiceId.From(value))
            .IsRequired();

        builder.Property(x => x.ProductVarietyId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.PackSize)
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

        builder.HasIndex("CustomerInvoiceId");
        builder.HasIndex(x => x.ProductVarietyId);
    }
}
