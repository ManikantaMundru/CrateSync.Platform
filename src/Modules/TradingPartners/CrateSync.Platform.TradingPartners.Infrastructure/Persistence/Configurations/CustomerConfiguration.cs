using CrateSync.Platform.TradingPartners.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.TradingPartners.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "partners");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CommercialIdentifier)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.DeactivatedAtUtc);

        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(200);

            address.Property(x => x.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(200);

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100);

            address.Property(x => x.StateOrProvince)
                .HasColumnName("StateOrProvince")
                .HasMaxLength(100);

            address.Property(x => x.PostalCode)
                .HasColumnName("PostalCode")
                .HasMaxLength(30);

            address.Property(x => x.Country)
                .HasColumnName("Country")
                .HasMaxLength(100);
        });

        builder.OwnsOne(x => x.ContactDetails, contact =>
        {
            contact.Property(x => x.ContactName)
                .HasColumnName("ContactName")
                .HasMaxLength(150);

            contact.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(254);

            contact.Property(x => x.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(50);
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.CommercialIdentifier
        })
        .IsUnique()
        .HasFilter("[CommercialIdentifier] IS NOT NULL");
    }
}
