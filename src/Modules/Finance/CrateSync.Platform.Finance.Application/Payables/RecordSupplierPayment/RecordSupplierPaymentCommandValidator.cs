using FluentValidation;

namespace CrateSync.Platform.Finance.Application.Payables.RecordSupplierPayment;

internal sealed class RecordSupplierPaymentCommandValidator
    : AbstractValidator<RecordSupplierPaymentCommand>
{
    public RecordSupplierPaymentCommandValidator()
    {
        RuleFor(x => x.SupplierInvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(100);
    }
}
