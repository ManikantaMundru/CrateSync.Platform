using FluentValidation;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.ActivateSupplier;

internal sealed class ActivateSupplierCommandValidator
    : AbstractValidator<ActivateSupplierCommand>
{
    public ActivateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
