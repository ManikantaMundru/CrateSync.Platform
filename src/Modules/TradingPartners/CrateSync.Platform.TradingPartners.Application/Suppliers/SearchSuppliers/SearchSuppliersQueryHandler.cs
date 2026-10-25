using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Abstractions;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.SearchSuppliers;

internal sealed class SearchSuppliersQueryHandler
    : IQueryHandler<SearchSuppliersQuery, IReadOnlyCollection<SupplierSummaryResponse>>
{
    private readonly ITradingPartnersReadService _readService;
    private readonly ITenantContext _tenantContext;

    public SearchSuppliersQueryHandler(ITradingPartnersReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public Task<IReadOnlyCollection<SupplierSummaryResponse>> Handle(
        SearchSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchSuppliersAsync(
            _tenantContext.TenantId,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);
    }
}
