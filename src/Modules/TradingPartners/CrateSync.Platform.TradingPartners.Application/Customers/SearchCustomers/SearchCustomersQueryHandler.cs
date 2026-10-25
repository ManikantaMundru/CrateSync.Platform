using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Abstractions;

namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

internal sealed class SearchCustomersQueryHandler
    : IQueryHandler<SearchCustomersQuery, IReadOnlyCollection<CustomerSummaryResponse>>
{
    private readonly ITradingPartnersReadService _readService;
    private readonly ITenantContext _tenantContext;

    public SearchCustomersQueryHandler(ITradingPartnersReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public Task<IReadOnlyCollection<CustomerSummaryResponse>> Handle(
        SearchCustomersQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchCustomersAsync(
            _tenantContext.TenantId,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);
    }
}
