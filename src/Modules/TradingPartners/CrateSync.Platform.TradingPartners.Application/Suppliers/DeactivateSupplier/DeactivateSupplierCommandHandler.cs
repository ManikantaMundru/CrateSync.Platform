using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.DeactivateSupplier;

internal sealed class DeactivateSupplierCommandHandler
    : ICommandHandler<DeactivateSupplierCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    public DeactivateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplierId = SupplierId.From(request.SupplierId);

        var supplier = await _supplierRepository.GetByIdAsync(supplierId, _tenantContext.TenantId, cancellationToken);

        if (supplier is null) return Result.Failure(SupplierErrors.NotFound(supplierId));

        supplier.Deactivate(_timeProvider.GetUtcNow());

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
