using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Customers;
using CrateSync.Platform.TradingPartners.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrateSync.Platform.TradingPartners.Infrastructure.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly TradingPartnersDbContext _dbContext;

    public CustomerRepository(TradingPartnersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customer?> GetByIdAsync(
        CustomerId customerId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .FirstOrDefaultAsync(
                x => x.Id == customerId && x.TenantId == tenantId,
                cancellationToken);
    }

    public Task<bool> CommercialIdentifierExistsAsync(
        Guid tenantId,
        string commercialIdentifier,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .AnyAsync(
                x =>
                    x.TenantId == tenantId &&
                    x.CommercialIdentifier == commercialIdentifier,
                cancellationToken);
    }

    public void Add(Customer customer)
    {
        _dbContext.Customers.Add(customer);
    }
}
