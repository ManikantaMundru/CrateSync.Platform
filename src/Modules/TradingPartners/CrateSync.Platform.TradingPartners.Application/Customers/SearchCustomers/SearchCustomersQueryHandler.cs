using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Common;

namespace CrateSync.Platform.TradingPartners.Application.Customers.SearchCustomers;

internal sealed class SearchCustomersQueryHandler
    : IQueryHandler<SearchCustomersQuery, PagedResponse<CustomerSummaryResponse>>
{
    private readonly ITradingPartnersReadService _readService;
    private readonly ITenantContext _tenantContext;

    public SearchCustomersQueryHandler(ITradingPartnersReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public Task<PagedResponse<CustomerSummaryResponse>> Handle(
        SearchCustomersQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchCustomersAsync(
            _tenantContext.TenantId,
            request.SearchTerm,
            request.IsActive,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
