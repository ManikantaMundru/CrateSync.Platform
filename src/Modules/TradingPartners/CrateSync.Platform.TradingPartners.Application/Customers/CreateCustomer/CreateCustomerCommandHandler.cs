using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Customers.Common;
using CrateSync.Platform.TradingPartners.Domain.Customers;
using CrateSync.Platform.TradingPartners.Domain.ValueObjects;

namespace CrateSync.Platform.TradingPartners.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandHandler: ICommandHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly TimeProvider _timeProvider;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        TimeProvider timeProvider)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        if (!string.IsNullOrWhiteSpace(request.CommercialIdentifier))
        {
            var exists = await _customerRepository.CommercialIdentifierExistsAsync(
                tenantId,
                request.CommercialIdentifier,
                cancellationToken);

            if (exists) return Result<Guid>.Failure(CustomerErrors.DuplicateCommercialIdentifier(request.CommercialIdentifier));
        }

        var address = CreateAddress(request);
        var contactDetails = CreateContactDetails(request);

        var customer = Customer.Create(
            tenantId,
            request.Name,
            request.CommercialIdentifier,
            address,
            contactDetails,
            _timeProvider.GetUtcNow());

        _customerRepository.Add(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(customer.Id.Value);
    }

    private static Address? CreateAddress(CreateCustomerCommand request)
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

    private static ContactDetails? CreateContactDetails(CreateCustomerCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone)) return null;

        return new ContactDetails(request.ContactName, request.Email, request.Phone);
    }
}
