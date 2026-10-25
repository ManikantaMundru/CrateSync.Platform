using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration
    : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(
        EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements", "inventory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => StockMovementId.From(value));

        builder.Property<InventoryBalanceId>("InventoryBalanceId")
            .HasConversion(
                id => id.Value,
                value => InventoryBalanceId.From(value))
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.OnHandChange)
            .IsRequired();

        builder.Property(x => x.ReservedChange)
            .IsRequired();

        builder.Property(x => x.OnHandAfter)
            .IsRequired();

        builder.Property(x => x.ReservedAfter)
            .IsRequired();

        builder.OwnsOne(x => x.Reference, reference =>
        {
            reference.Property(x => x.Type)
                .HasColumnName("ReferenceType")
                .HasMaxLength(50)
                .IsRequired();

            reference.Property(x => x.Id)
                .HasColumnName("ReferenceId")
                .IsRequired();

            reference.HasIndex(x => new
            {
                x.Type,
                x.Id
            });
        });

        builder.Property(x => x.Reason)
            .HasMaxLength(250);

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex("InventoryBalanceId");

        builder.HasIndex(x => x.OccurredAtUtc);
    }
}
