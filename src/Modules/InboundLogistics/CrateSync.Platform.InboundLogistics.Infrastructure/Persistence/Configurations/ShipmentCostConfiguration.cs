using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence.Configurations;
internal sealed class ShipmentExpenseConfiguration : IEntityTypeConfiguration<ShipmentExpense>
{
    public void Configure(EntityTypeBuilder<ShipmentExpense> b)
    {
        b.ToTable("ShipmentExpenses", "logistics");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property<InboundShipmentId>("ShipmentId").HasConversion(x => x.Value, x => InboundShipmentId.From(x)).IsRequired();
        b.Property(x => x.Category).HasMaxLength(40).IsRequired();
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.ExchangeRate).HasColumnType("decimal(18,8)");
        b.Property(x => x.Reference).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500).IsRequired();
        b.Property(x => x.ReversalReason).HasMaxLength(500);
        b.HasIndex("ShipmentId");
    }
}
internal sealed class ShipmentCostExchangeRateConfiguration : IEntityTypeConfiguration<ShipmentCostExchangeRate>
{
    public void Configure(EntityTypeBuilder<ShipmentCostExchangeRate> b)
    {
        b.ToTable("ShipmentCostExchangeRates", "logistics");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property<InboundShipmentId>("ShipmentId").HasConversion(x => x.Value, x => InboundShipmentId.From(x)).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.Rate).HasColumnType("decimal(18,8)");
        b.HasIndex("ShipmentId", "Currency").IsUnique();
    }
}
