using CrateSync.Platform.InboundLogistics.Domain.Shipments;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.InboundLogistics.Infrastructure.Persistence;

public sealed class InboundLogisticsDbContext(DbContextOptions<InboundLogisticsDbContext> options) : DbContext(options)
{
    public DbSet<InboundShipment> InboundShipments => Set<InboundShipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("logistics");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboundLogisticsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
