using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;
using CrateSync.Platform.TradingPartners.Domain.ValueObjects;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.UpdateSupplier;

internal sealed class UpdateSupplierCommandHandler
    : ICommandHandler<UpdateSupplierCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(
        UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var supplierId = SupplierId.From(request.SupplierId);

        var supplier = await _supplierRepository.GetByIdAsync(
            supplierId,
            tenantId,
            cancellationToken);

        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound(supplierId));

        if (CommercialIdentifierChanged(
                supplier.CommercialIdentifier,
                request.CommercialIdentifier) &&
            !string.IsNullOrWhiteSpace(request.CommercialIdentifier))
        {
            var exists = await _supplierRepository.CommercialIdentifierExistsAsync(
                tenantId,
                request.CommercialIdentifier,
                cancellationToken);

            if (exists)
                return Result.Failure(
                    SupplierErrors.DuplicateCommercialIdentifier(
                        request.CommercialIdentifier));
        }

        var address = CreateAddress(request);
        var contactDetails = CreateContactDetails(request);

        supplier.UpdateDetails(
            request.Name,
            request.CommercialIdentifier,
            address,
            contactDetails);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool CommercialIdentifierChanged(
        string? current,
        string? requested)
    {
        return !string.Equals(
            current?.Trim(),
            requested?.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }

    private static Address? CreateAddress(UpdateSupplierCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.AddressLine1))
            return null;

        return new Address(
            request.AddressLine1,
            request.AddressLine2,
            request.City!,
            request.StateOrProvince,
            request.PostalCode!,
            request.Country!);
    }

    private static ContactDetails? CreateContactDetails(
        UpdateSupplierCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) &&
            string.IsNullOrWhiteSpace(request.Phone))
            return null;

        return new ContactDetails(
            request.ContactName,
            request.Email,
            request.Phone);
    }
}
