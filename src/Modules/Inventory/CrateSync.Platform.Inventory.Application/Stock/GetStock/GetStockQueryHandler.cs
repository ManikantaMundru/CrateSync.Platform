using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Inventory.Application.Abstractions;
using CrateSync.Platform.Inventory.Application.Stock.Common;

namespace CrateSync.Platform.Inventory.Application.Stock.GetStock;

internal sealed class GetStockQueryHandler(
    IInventoryReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<GetStockQuery, Result<StockResponse>>
{
    public async Task<Result<StockResponse>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var response = await readService.GetStockAsync(
            tenantContext.TenantId,
            request.WarehouseId,
            request.ProductVarietyId,
            request.OriginCountry,
            cancellationToken);

        return response is null
            ? Result<StockResponse>.Failure(InventoryErrors.StockNotFound)
            : Result<StockResponse>.Success(response);
    }
}
