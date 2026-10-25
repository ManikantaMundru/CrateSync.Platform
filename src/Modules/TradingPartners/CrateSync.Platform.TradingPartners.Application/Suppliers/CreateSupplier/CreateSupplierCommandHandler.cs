using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Domain.Suppliers;
using CrateSync.Platform.TradingPartners.Domain.ValueObjects;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.CreateSupplier;

internal sealed class CreateSupplierCommandHandler
    : ICommandHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    public CreateSupplierCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        if (!string.IsNullOrWhiteSpace(request.CommercialIdentifier))
        {
            var exists = await _supplierRepository.CommercialIdentifierExistsAsync(
                tenantId,
                request.CommercialIdentifier,
                cancellationToken);

            if (exists) return Result<Guid>.Failure(SupplierErrors.DuplicateCommercialIdentifier(request.CommercialIdentifier));
        }

        var address = CreateAddress(request);
        var contactDetails = CreateContactDetails(request);

        var supplier = Supplier.Create(
            tenantId,
            request.Name,
            request.CommercialIdentifier,
            address,
            contactDetails,
            _timeProvider.GetUtcNow());

        _supplierRepository.Add(supplier);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplier.Id.Value);
    }

    private static Address? CreateAddress(CreateSupplierCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.AddressLine1)) return null;

        return new Address(
            request.AddressLine1,
            request.AddressLine2,
            request.City ?? string.Empty,
            request.StateOrProvince,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty);
    }

    private static ContactDetails? CreateContactDetails(CreateSupplierCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone)) return null;

        return new ContactDetails(request.ContactName, request.Email, request.Phone);
    }
}
