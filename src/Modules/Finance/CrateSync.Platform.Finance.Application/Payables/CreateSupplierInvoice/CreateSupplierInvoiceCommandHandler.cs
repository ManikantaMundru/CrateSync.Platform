using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Finance.Application.Abstractions;
using CrateSync.Platform.Finance.Application.Common;
using CrateSync.Platform.Finance.Domain.Common;
using CrateSync.Platform.Finance.Domain.Payables;

namespace CrateSync.Platform.Finance.Application.Payables.CreateSupplierInvoice;

internal sealed class CreateSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository repository,
    IFinanceUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateSupplierInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateSupplierInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.InvoiceNumberExistsAsync(
                tenantContext.TenantId,
                request.InvoiceNumber,
                cancellationToken))
        {
            return Result<Guid>.Failure(
                FinanceErrors.InvoiceNumberExists);
        }

        var currency = new Currency(request.Currency);
        var now = timeProvider.GetUtcNow();

        var invoice = SupplierInvoice.Create(
            tenantContext.TenantId,
            request.SupplierId,
            request.ShipmentId,
            request.InvoiceNumber,
            currency,
            request.Lines.Select(x =>
                new SupplierInvoiceLineSnapshot(
                    x.Description,
                    x.ProductVarietyId,
                    x.Quantity,
                    x.UnitCost,
                    currency.Code)),
            request.InvoiceDateUtc,
            now);

        repository.Add(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(invoice.Id.Value);
    }
}
