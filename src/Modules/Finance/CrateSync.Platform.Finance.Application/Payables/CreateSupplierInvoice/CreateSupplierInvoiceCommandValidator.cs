using FluentValidation;

namespace CrateSync.Platform.Finance.Application.Payables.CreateSupplierInvoice;

internal sealed class CreateSupplierInvoiceCommandValidator
    : AbstractValidator<CreateSupplierInvoiceCommand>
{
    public CreateSupplierInvoiceCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Lines).NotEmpty();

        RuleForEach(x => x.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(x => x.Description)
                    .NotEmpty()
                    .MaximumLength(250);

                line.RuleFor(x => x.Quantity)
                    .GreaterThan(0);

                line.RuleFor(x => x.UnitCost)
                    .GreaterThanOrEqualTo(0);
            });
    }
}
