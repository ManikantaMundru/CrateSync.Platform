using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Configurations;
internal sealed class SellingPriceChangeConfiguration : IEntityTypeConfiguration<SellingPriceChange>
{
    public void Configure(EntityTypeBuilder<SellingPriceChange> builder)
    {
        builder.ToTable("SellingPriceChanges", "inventory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property<InventoryBalanceId>("InventoryBalanceId").HasConversion(x => x.Value, x => InventoryBalanceId.From(x)).IsRequired();
        builder.Property(x => x.PreviousPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PreviousCurrency).HasMaxLength(3);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.HasIndex("InventoryBalanceId");
    }
}
