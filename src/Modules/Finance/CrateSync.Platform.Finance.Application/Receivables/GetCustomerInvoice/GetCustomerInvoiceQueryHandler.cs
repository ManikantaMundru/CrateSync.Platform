using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.GetCustomerInvoice;

internal sealed class GetCustomerInvoiceQueryHandler(
    IFinanceReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<GetCustomerInvoiceQuery, Result<CustomerInvoiceResponse>>
{
    public async Task<Result<CustomerInvoiceResponse>> Handle(
        GetCustomerInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await readService.GetCustomerInvoiceAsync(
            tenantContext.TenantId,
            request.CustomerInvoiceId,
            cancellationToken);

        return invoice is null
            ? Result<CustomerInvoiceResponse>.Failure(
                FinanceErrors.CustomerInvoiceNotFound)
            : Result<CustomerInvoiceResponse>.Success(invoice);
    }
}
