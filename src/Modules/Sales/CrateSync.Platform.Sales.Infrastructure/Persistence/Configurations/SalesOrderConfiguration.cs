using CrateSync.Platform.Sales.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrateSync.Platform.Sales.Infrastructure.Persistence.Configurations;

internal sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders", "sales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SalesOrderId.From(value));

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.WarehouseId).IsRequired();

        builder.Property(x => x.OrderNumber)
            .HasConversion(orderNumber => orderNumber.Value, value => new OrderNumber(value))
            .HasColumnName("OrderNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ReservationFailureReason)
            .HasMaxLength(500);

        builder.OwnsOne(x => x.DeliveryAddress, address =>
        {
            address.Property(x => x.AddressLine1)
                .HasColumnName("DeliveryAddressLine1")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(x => x.AddressLine2)
                .HasColumnName("DeliveryAddressLine2")
                .HasMaxLength(200);

            address.Property(x => x.City)
                .HasColumnName("DeliveryCity")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.Province)
                .HasColumnName("DeliveryProvince")
                .HasMaxLength(100);

            address.Property(x => x.PostalCode)
                .HasColumnName("DeliveryPostalCode")
                .HasMaxLength(20);

            address.Property(x => x.Country)
                .HasColumnName("DeliveryCountry")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.ConfirmedAtUtc);
        builder.Property(x => x.AllocatedAtUtc);
        builder.Property(x => x.DispatchedAtUtc);
        builder.Property(x => x.FulfilledAtUtc);
        builder.Property(x => x.CancelledAtUtc);

        builder.Ignore(x => x.TotalAmount);

        builder.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.WarehouseId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("SalesOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
