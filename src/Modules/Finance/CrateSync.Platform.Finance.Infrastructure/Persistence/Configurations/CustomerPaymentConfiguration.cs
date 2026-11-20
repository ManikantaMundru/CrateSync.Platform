using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence.Configurations;

internal sealed class CustomerPaymentConfiguration
    : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(
        EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.ToTable("CustomerPayments", "finance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerPaymentId.From(value));

        builder.Property<CustomerInvoiceId>("CustomerInvoiceId")
            .HasConversion(
                id => id.Value,
                value => CustomerInvoiceId.From(value))
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

        builder.HasIndex("CustomerInvoiceId");
        builder.HasIndex(x => x.PaidAtUtc);
    }
}
