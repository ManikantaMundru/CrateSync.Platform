using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.VoidCustomerInvoice;

internal sealed class VoidCustomerInvoiceCommandHandler(
    ICustomerInvoiceRepository repository,
    IFinanceUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<VoidCustomerInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        VoidCustomerInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(
            CustomerInvoiceId.From(request.CustomerInvoiceId),
            tenantContext.TenantId,
            cancellationToken);

        if (invoice is null)
            return Result.Failure(
                FinanceErrors.CustomerInvoiceNotFound);

        try
        {
            invoice.Void(timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                FinanceErrors.Conflict(
                    "Finance.InvalidCustomerInvoiceState",
                    ex.Message));
        }
    }
}
