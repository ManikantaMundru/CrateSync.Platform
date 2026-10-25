using System.Reflection.Emit;
using CrateSync.Platform.Inventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
