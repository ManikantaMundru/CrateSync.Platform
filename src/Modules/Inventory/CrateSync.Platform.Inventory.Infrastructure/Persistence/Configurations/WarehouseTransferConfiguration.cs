using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CrateSync.Platform.Inventory.Infrastructure.Persistence.Configurations;
internal sealed class WarehouseTransferConfiguration : IEntityTypeConfiguration<WarehouseTransfer>
{
    public void Configure(EntityTypeBuilder<WarehouseTransfer> b)
    {
        b.ToTable("WarehouseTransfers", "inventory");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Status).HasMaxLength(40).IsConcurrencyToken();
        b.Property(x => x.OriginCountry).HasMaxLength(2);
        b.Property(x => x.BatchCode).HasMaxLength(100);
        b.Property(x => x.PackWeightKg).HasColumnType("decimal(18,3)");
        b.Property(x => x.TransportCompany).HasMaxLength(150);
        b.Property(x => x.DriverName).HasMaxLength(150);
        b.Property(x => x.DriverPhone).HasMaxLength(50);
        b.Property(x => x.VehicleNumber).HasMaxLength(50);
        b.Property(x => x.TransferReference).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(250);
        b.Property(x => x.ReceivingRemarks).HasMaxLength(500);
        b.HasIndex(x => new { x.TenantId, x.SourceWarehouseId });
        b.HasIndex(x => new { x.TenantId, x.DestinationWarehouseId });
    }
}
