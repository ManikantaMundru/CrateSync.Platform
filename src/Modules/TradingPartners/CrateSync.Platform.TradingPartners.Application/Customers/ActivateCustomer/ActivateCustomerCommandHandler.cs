using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Customers.Common;
using CrateSync.Platform.TradingPartners.Domain.Customers;

namespace CrateSync.Platform.TradingPartners.Application.Customers.ActivateCustomer;

internal sealed class ActivateCustomerCommandHandler
    : ICommandHandler<ActivateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITradingPartnersUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ActivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ITradingPartnersUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(
        ActivateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customerId = CustomerId.From(request.CustomerId);

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            _tenantContext.TenantId,
            cancellationToken);

        if (customer is null)
            return Result.Failure(
                CustomerErrors.NotFound(customerId));

        customer.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
