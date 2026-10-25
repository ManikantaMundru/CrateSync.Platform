using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;

namespace CrateSync.Platform.Inventory.Application.Stock.GetStockMovements;

internal sealed class GetStockMovementsQueryHandler(
    IInventoryReadService readService,
    ITenantContext tenantContext) :
    IQueryHandler<GetStockMovementsQuery, IReadOnlyCollection<StockMovementResponse>>
{
    private readonly IInventoryReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public Task<IReadOnlyCollection<StockMovementResponse>> Handle(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken)
    {
        return _readService.GetMovementsAsync(
            _tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);
    }
}
