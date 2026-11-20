using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence.Configurations;

internal sealed class SupplierPaymentConfiguration
    : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(
        EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.ToTable("SupplierPayments", "finance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => SupplierPaymentId.From(value));

        builder.Property<SupplierInvoiceId>("SupplierInvoiceId")
            .HasConversion(
                id => id.Value,
                value => SupplierInvoiceId.From(value))
            .IsRequired();

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(x => x.Amount)
                .HasColumnName("Amount")
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

        builder.Property(x => x.Reference)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PaidAtUtc)
            .IsRequired();

        builder.HasIndex("SupplierInvoiceId");
        builder.HasIndex(x => x.PaidAtUtc);
    }
}
