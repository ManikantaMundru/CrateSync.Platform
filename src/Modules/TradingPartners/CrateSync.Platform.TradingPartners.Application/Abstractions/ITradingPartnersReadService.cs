using CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;
using CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;
using CrateSync.Platform.TradingPartners.Application.Suppliers.GetSupplier;
using CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

namespace CrateSync.Platform.TradingPartners.Application.Abstractions
{
    public interface ITradingPartnersReadService
    {
        Task<CustomerResponse?> GetCustomerByIdAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CustomerSummaryResponse>> SearchCustomersAsync(
            Guid tenantId,
            string? searchTerm,
            bool? isActive,
            CancellationToken cancellationToken = default);

        Task<SupplierResponse?> GetSupplierByIdAsync(Guid tenantId, Guid supplierId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<SupplierSummaryResponse>> SearchSuppliersAsync(
            Guid tenantId,
            string? searchTerm,
            bool? isActive,
            CancellationToken cancellationToken = default);
    }
}
