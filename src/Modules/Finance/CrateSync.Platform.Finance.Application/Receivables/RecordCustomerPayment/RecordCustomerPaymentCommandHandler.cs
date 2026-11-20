using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;
using CrateSync.Platform.Finance.Domain.Common;

namespace CrateSync.Platform.Finance.Application.Receivables.RecordCustomerPayment;

internal sealed class RecordCustomerPaymentCommandHandler(
    ICustomerInvoiceRepository repository,
    IFinanceUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<RecordCustomerPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        RecordCustomerPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await repository.GetByIdAsync(
            CustomerInvoiceId.From(request.CustomerInvoiceId),
            tenantContext.TenantId,
            cancellationToken);

        if (invoice is null)
            return Result<Guid>.Failure(
                FinanceErrors.CustomerInvoiceNotFound);

        try
        {
            var payment = invoice.RecordPayment(
                request.Amount,
                request.Reference,
                timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(payment.Id.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(
                FinanceErrors.Conflict(
                    "Finance.InvalidCustomerPayment",
                    ex.Message));
        }
    }
}
