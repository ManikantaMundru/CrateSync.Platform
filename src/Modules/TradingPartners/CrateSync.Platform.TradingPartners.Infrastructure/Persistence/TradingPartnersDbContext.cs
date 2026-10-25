using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Customers;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.TradingPartners.Infrastructure.Persistence;

public sealed class TradingPartnersDbContext(DbContextOptions<TradingPartnersDbContext> options) 
    : DbContext(options), ITradingPartnersUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("partners");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingPartnersDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
