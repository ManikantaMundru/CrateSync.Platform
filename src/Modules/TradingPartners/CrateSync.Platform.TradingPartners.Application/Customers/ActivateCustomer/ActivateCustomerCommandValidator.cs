using FluentValidation;

namespace CrateSync.Platform.TradingPartners.Application.Customers.ActivateCustomer;

internal sealed class ActivateCustomerCommandValidator
    : AbstractValidator<ActivateCustomerCommand>
{
    public ActivateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
