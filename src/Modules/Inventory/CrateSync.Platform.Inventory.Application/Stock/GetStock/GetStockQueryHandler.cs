using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;

namespace CrateSync.Platform.Inventory.Application.Stock.GetStock;

internal sealed class GetStockQueryHandler(
    IInventoryReadService readService,
    ITenantContext tenantContext) :
    IQueryHandler<GetStockQuery, Result<StockResponse>>
{
    private readonly IInventoryReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<Result<StockResponse>> Handle(
        GetStockQuery request,
        CancellationToken cancellationToken)
    {
        var stock = await _readService.GetStockAsync(
            _tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            cancellationToken);

        return stock is null
            ? Result<StockResponse>.Failure(
                new Error(
                    "Inventory.StockNotFound",
                    "Inventory balance was not found.",
                    ErrorType.NotFound))
            : Result<StockResponse>.Success(stock);
    }
}
