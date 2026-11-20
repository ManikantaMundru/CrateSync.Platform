using CrateSync.Platform.Finance.Domain.Payables;
using CrateSync.Platform.Finance.Domain.Receivables;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.Finance.Infrastructure.Persistence;

public sealed class FinanceDbContext(
    DbContextOptions<FinanceDbContext> options)
    : DbContext(options)
{
    public DbSet<CustomerInvoice> CustomerInvoices => Set<CustomerInvoice>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FinanceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
