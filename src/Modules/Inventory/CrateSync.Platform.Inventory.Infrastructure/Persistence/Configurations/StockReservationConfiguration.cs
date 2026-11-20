using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations", "inventory");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => StockReservationId.From(value));

        builder.Property<InventoryBalanceId>("InventoryBalanceId")
            .HasConversion(id => id.Value, value => InventoryBalanceId.From(value))
            .IsRequired();

        builder.Property(x => x.SalesOrderId).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CompletedAtUtc);

        builder.HasIndex(x => new { x.SalesOrderId, x.Status });
        builder.HasIndex("InventoryBalanceId", nameof(StockReservation.SalesOrderId)).IsUnique();
    }
}
