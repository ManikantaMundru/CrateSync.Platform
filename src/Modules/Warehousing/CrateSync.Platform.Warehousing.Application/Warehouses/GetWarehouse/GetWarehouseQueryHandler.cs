using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Application.Warehouses.common;
using CrateSync.Platform.Warehousing.Domain.Warehouses;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.GetWarehouse;

internal sealed class GetWarehouseQueryHandler(
    IWarehousingReadService readService, 
    ITenantContext tenantContext) : IQueryHandler<GetWarehouseQuery, Result<WarehouseResponse>>
{
    private readonly IWarehousingReadService _readService = readService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<Result<WarehouseResponse>> Handle(GetWarehouseQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _readService.GetByIdAsync(_tenantContext.TenantId, request.WarehouseId, cancellationToken);

        if (warehouse is null)
            return Result<WarehouseResponse>.Failure(WarehouseErrors.NotFound(WarehouseId.From(request.WarehouseId)));

        return Result<WarehouseResponse>.Success(warehouse);
    }
}
