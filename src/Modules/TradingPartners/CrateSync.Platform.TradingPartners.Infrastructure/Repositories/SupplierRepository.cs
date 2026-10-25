using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;
using CrateSync.Platform.TradingPartners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.TradingPartners.Infrastructure.Repositories;

internal sealed class SupplierRepository : ISupplierRepository
{
    private readonly TradingPartnersDbContext _dbContext;

    public SupplierRepository(TradingPartnersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Supplier?> GetByIdAsync(
        SupplierId supplierId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers
            .FirstOrDefaultAsync(
                x => x.Id == supplierId && x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> CommercialIdentifierExistsAsync(
        Guid tenantId,
        string commercialIdentifier,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.CommercialIdentifier == commercialIdentifier,
                cancellationToken);
    }

    public void Add(Supplier supplier)
    {
        _dbContext.Suppliers.Add(supplier);
    }
}
