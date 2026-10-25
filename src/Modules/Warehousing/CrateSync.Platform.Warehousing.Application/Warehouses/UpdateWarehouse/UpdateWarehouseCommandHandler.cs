using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Application.Warehouses.common;
using CrateSync.Platform.Warehousing.Domain.ValueObjects;
using CrateSync.Platform.Warehousing.Domain.Warehouses;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.UpdateWarehouse;

internal sealed class UpdateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IWarehousingUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateWarehouseCommand, Result>
{
    private readonly IWarehouseRepository _warehouseRepository = warehouseRepository;
    private readonly IWarehousingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var warehouseId = WarehouseId.From(request.WarehouseId);

        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, tenantId, cancellationToken);

        if (warehouse is null) return Result.Failure(WarehouseErrors.NotFound(warehouseId));

        var code = request.Code.Trim().ToUpperInvariant();

        if (!string.Equals(warehouse.Code, code, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _warehouseRepository.CodeExistsAsync(tenantId, code, cancellationToken);

            if (exists) return Result.Failure(WarehouseErrors.DuplicateCode(code));
        }

        var address = new WarehouseAddress(
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.PostalCode,
            request.Country);

        warehouse.UpdateDetails(
            request.Name,
            code,
            address,
            _timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
