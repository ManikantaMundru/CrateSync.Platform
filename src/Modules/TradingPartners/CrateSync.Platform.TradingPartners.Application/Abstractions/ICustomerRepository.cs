using CrateSync.Platform.TradingPartners.Domain.Customers;

namespace CrateSync.Platform.TradingPartners.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId customerId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CommercialIdentifierExistsAsync(Guid tenantId, string commercialIdentifier, CancellationToken cancellationToken = default);

    void Add(Customer customer);
}
