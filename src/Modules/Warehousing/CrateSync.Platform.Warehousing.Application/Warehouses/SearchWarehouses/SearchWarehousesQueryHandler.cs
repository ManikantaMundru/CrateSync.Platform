using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Warehousing.Application.Abstractions;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.SearchWarehouses;

internal sealed class SearchWarehousesQueryHandler(
    IWarehousingReadService readService,
    ITenantContext tenantContext) : IQueryHandler<SearchWarehousesQuery, IReadOnlyCollection<WarehouseSummaryResponse>>
{
    private readonly IWarehousingReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public Task<IReadOnlyCollection<WarehouseSummaryResponse>> Handle(
        SearchWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchAsync(
            _tenantContext.TenantId,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);
    }
}
