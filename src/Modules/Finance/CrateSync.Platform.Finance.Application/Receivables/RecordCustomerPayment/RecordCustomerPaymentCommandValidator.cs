using FluentValidation;

namespace CrateSync.Platform.Finance.Application.Receivables.RecordCustomerPayment;

internal sealed class RecordCustomerPaymentCommandValidator
    : AbstractValidator<RecordCustomerPaymentCommand>
{
    public RecordCustomerPaymentCommandValidator()
    {
        RuleFor(x => x.CustomerInvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(100);
    }
}
