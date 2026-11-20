using CrateSync.Platform.Tenancy.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Tenancy.Infrastructure.Persistence;

public sealed class TenancyDbContext(
    DbContextOptions<TenancyDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tenancy");
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TenancyDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
