using CrateSync.Platform.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductVarietyConfiguration : IEntityTypeConfiguration<ProductVariety>
{
    public void Configure(EntityTypeBuilder<ProductVariety> builder)
    {
        builder.ToTable("ProductVarieties", "catalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ProductVarietyId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex("ProductId", nameof(ProductVariety.Name))
            .IsUnique();
    }
}
