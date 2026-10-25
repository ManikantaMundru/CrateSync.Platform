using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Warehousing.Infrastructure.Persistence;

public sealed class WarehousingDbContext(DbContextOptions<WarehousingDbContext> options) : DbContext(options), IWarehousingUnitOfWork
{
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("warehousing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehousingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
