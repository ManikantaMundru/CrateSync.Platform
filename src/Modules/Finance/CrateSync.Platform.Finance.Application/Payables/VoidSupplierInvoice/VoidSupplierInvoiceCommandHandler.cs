using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Payables.VoidSupplierInvoice;

internal sealed class VoidSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository repository,
    IFinanceUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<VoidSupplierInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        VoidSupplierInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(
            SupplierInvoiceId.From(request.SupplierInvoiceId),
            tenantContext.TenantId,
            cancellationToken);

        if (invoice is null)
            return Result.Failure(
                FinanceErrors.SupplierInvoiceNotFound);

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
                    "Finance.InvalidSupplierInvoiceState",
                    ex.Message));
        }
    }
}
