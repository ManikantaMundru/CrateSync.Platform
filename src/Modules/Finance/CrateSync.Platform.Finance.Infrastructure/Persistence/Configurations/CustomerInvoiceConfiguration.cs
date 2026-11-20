using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Receivables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence.Configurations;

internal sealed class CustomerInvoiceConfiguration
    : IEntityTypeConfiguration<CustomerInvoice>
{
    public void Configure(EntityTypeBuilder<CustomerInvoice> builder)
    {
        builder.ToTable("CustomerInvoices", "finance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerInvoiceId.From(value));

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.SalesOrderId).IsRequired();

        builder.Property(x => x.InvoiceNumber)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(
                currency => currency.Code,
                value => new Currency(value))
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.InvoiceDateUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.VoidAtUtc);

        builder.Ignore(x => x.TotalAmount);
        builder.Ignore(x => x.AmountPaid);
        builder.Ignore(x => x.OutstandingAmount);

        builder.HasIndex(x => new { x.TenantId, x.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.SalesOrderId })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.InvoiceDateUtc });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("CustomerInvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Payments)
            .WithOne()
            .HasForeignKey("CustomerInvoiceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
