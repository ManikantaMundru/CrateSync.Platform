using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.ActivateSupplier;

internal sealed class ActivateSupplierCommandHandler
    : ICommandHandler<ActivateSupplierCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ActivateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(
        ActivateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplierId = SupplierId.From(request.SupplierId);

        var supplier = await _supplierRepository.GetByIdAsync(
            supplierId,
            _tenantContext.TenantId,
            cancellationToken);

        if (supplier is null)
            return Result.Failure(
                SupplierErrors.NotFound(supplierId));

        supplier.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
