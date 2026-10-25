using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Customers.Common;
using CrateSync.Platform.TradingPartners.Domain.Customers;
using CrateSync.Platform.TradingPartners.Domain.ValueObjects;

namespace CrateSync.Platform.TradingPartners.Application.Customers.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler
    : ICommandHandler<UpdateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var customerId = CustomerId.From(request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            tenantId,
            cancellationToken);

        if (customer is null)
            return Result.Failure(CustomerErrors.NotFound(customerId));

        if (CommercialIdentifierChanged(
                customer.CommercialIdentifier,
                request.CommercialIdentifier) &&
            !string.IsNullOrWhiteSpace(request.CommercialIdentifier))
        {
            var exists = await _customerRepository.CommercialIdentifierExistsAsync(
                tenantId,
                request.CommercialIdentifier,
                cancellationToken);

            if (exists)
                return Result.Failure(
                    CustomerErrors.DuplicateCommercialIdentifier(
                        request.CommercialIdentifier));
        }

        var address = CreateAddress(request);
        var contactDetails = CreateContactDetails(request);

        customer.UpdateDetails(
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

    private static Address? CreateAddress(UpdateCustomerCommand request)
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
        UpdateCustomerCommand request)
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
