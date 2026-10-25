using CrateSync.Platform.TradingPartners.Domain.Suppliers;

namespace CrateSync.Platform.TradingPartners.Application.Abstractions;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(SupplierId supplierId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<bool> CommercialIdentifierExistsAsync(Guid tenantId, string commercialIdentifier, CancellationToken cancellationToken = default);

    void Add(Supplier supplier);
}
