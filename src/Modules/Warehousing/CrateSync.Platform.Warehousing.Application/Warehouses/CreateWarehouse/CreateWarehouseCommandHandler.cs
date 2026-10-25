using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Warehousing.Application.Abstractions;
using CrateSync.Platform.Warehousing.Application.Warehouses.common;
using CrateSync.Platform.Warehousing.Domain.ValueObjects;
using CrateSync.Platform.Warehousing.Domain.Warehouses;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.CreateWarehouse;

internal sealed class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IWarehousingUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider) : ICommandHandler<CreateWarehouseCommand, Result<Guid>>
{
    private readonly IWarehouseRepository _warehouseRepository = warehouseRepository;
    private readonly IWarehousingUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var code = request.Code.Trim().ToUpperInvariant();

        if (await _warehouseRepository.CodeExistsAsync(tenantId, code, cancellationToken))
            return Result<Guid>.Failure(WarehouseErrors.DuplicateCode(code));

        var address = new WarehouseAddress(
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.StateOrProvince,
            request.PostalCode,
            request.Country);

        var warehouse = Warehouse.Create(
            tenantId,
            request.Name,
            code,
            address,
            _timeProvider.GetUtcNow());

        _warehouseRepository.Add(warehouse);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(warehouse.Id.Value);
    }
}
