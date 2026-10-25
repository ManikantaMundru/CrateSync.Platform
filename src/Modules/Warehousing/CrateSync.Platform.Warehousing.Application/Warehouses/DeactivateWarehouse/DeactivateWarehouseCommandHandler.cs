using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Application.Warehouses.common;
using CrateSync.Platform.Warehousing.Domain.Warehouses;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.DeactivateWarehouse;

internal sealed class DeactivateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IWarehousingUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<DeactivateWarehouseCommand, Result>
{
    private readonly IWarehouseRepository _warehouseRepository = warehouseRepository;
    private readonly IWarehousingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = WarehouseId.From(request.WarehouseId);

        var warehouse = await _warehouseRepository.GetByIdAsync(
            warehouseId,
            _tenantContext.TenantId,
            cancellationToken);

        if (warehouse is null) return Result.Failure(WarehouseErrors.NotFound(warehouseId));

        warehouse.Deactivate(_timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
