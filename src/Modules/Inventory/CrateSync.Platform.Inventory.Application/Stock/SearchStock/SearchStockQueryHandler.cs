using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;

namespace CrateSync.Platform.Inventory.Application.Stock.SearchStock;

internal sealed class SearchStockQueryHandler(
    IInventoryReadService readService,
    ITenantContext tenantContext) :
    IQueryHandler<SearchStockQuery, IReadOnlyCollection<StockSummaryResponse>>
{
    private readonly IInventoryReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public Task<IReadOnlyCollection<StockSummaryResponse>> Handle(
        SearchStockQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.SearchStockAsync(
            _tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);
    }
}
