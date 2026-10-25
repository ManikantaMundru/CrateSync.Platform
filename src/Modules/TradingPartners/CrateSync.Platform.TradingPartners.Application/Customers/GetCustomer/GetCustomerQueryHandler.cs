using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.TradingPartners.Application.Abstractions;
using CrateSync.Platform.TradingPartners.Application.Customers.Common;
using CrateSync.Platform.TradingPartners.Domain.Customers;

namespace CrateSync.Platform.TradingPartners.Application.Customers.GetCustomer;

internal sealed class GetCustomerQueryHandler : IQueryHandler<GetCustomerQuery, Result<CustomerResponse>>
{
    private readonly ITradingPartnersReadService _readService;
    private readonly ITenantContext _tenantContext;

    public GetCustomerQueryHandler(ITradingPartnersReadService readService, ITenantContext tenantContext)
    {
        _readService = readService;
        _tenantContext = tenantContext;
    }

    public async Task<Result<CustomerResponse>> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _readService.GetCustomerByIdAsync(_tenantContext.TenantId, request.CustomerId, cancellationToken);

        if (customer is null) return Result<CustomerResponse>.Failure(CustomerErrors.NotFound(CustomerId.From(request.CustomerId)));

        return Result<CustomerResponse>.Success(customer);
    }
}
