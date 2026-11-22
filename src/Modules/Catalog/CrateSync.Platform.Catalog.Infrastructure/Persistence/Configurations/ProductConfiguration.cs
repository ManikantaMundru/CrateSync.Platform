using CrateSync.Platform.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ProductId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsActive).IsRequired();

        builder.Property(x => x.CustomsTariffCode)
            .HasMaxLength(20);

        builder.Property(x => x.ScientificName)
            .HasMaxLength(200);

        builder.Property(x => x.RegulatoryCategory)
            .HasMaxLength(100);

        builder.Property(x => x.DefaultUnitOfMeasure)
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.Name
        })
        .IsUnique();

        builder.HasMany(x => x.Varieties)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Varieties)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field);
    }
}
