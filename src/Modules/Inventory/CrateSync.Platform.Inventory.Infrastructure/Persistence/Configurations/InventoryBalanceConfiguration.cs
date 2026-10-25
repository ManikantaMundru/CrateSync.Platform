using CrateSync.Platform.Inventory.Domain.Stock;
using CrateSync.Platform.Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryBalanceConfiguration
    : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(
        EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances", "inventory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => InventoryBalanceId.From(value));

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.ProductVarietyId)
            .IsRequired();

        builder.Property(x => x.OnHand)
            .HasConversion(
                quantity => quantity.Value,
                value => new StockQuantity(value))
            .HasColumnName("OnHand")
            .IsRequired();

        builder.Property(x => x.Reserved)
            .HasConversion(
                quantity => quantity.Value,
                value => new StockQuantity(value))
            .HasColumnName("Reserved")
            .IsRequired();

        builder.Ignore(x => x.Available);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.WarehouseId,
            x.ProductVarietyId
        })
        .IsUnique();

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.WarehouseId
        });

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.ProductVarietyId
        });

        builder.HasMany(x => x.Movements)
            .WithOne()
            .HasForeignKey("InventoryBalanceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
