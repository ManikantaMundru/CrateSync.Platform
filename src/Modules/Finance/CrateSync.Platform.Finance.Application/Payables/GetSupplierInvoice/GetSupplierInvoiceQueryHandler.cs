using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.BuildingBlocks.Application.Queries;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;

namespace CrateSync.Platform.Finance.Application.Payables.GetSupplierInvoice;

internal sealed class GetSupplierInvoiceQueryHandler(
    IFinanceReadService readService,
    ITenantContext tenantContext)
    : IQueryHandler<GetSupplierInvoiceQuery, Result<SupplierInvoiceResponse>>
{
    public async Task<Result<SupplierInvoiceResponse>> Handle(
        GetSupplierInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var invoice = await readService.GetSupplierInvoiceAsync(
            tenantContext.TenantId,
            request.SupplierInvoiceId,
            cancellationToken);

        return invoice is null
            ? Result<SupplierInvoiceResponse>.Failure(
                FinanceErrors.SupplierInvoiceNotFound)
            : Result<SupplierInvoiceResponse>.Success(invoice);
    }
}
