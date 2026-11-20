using CrateSync.Platform.Sales.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
