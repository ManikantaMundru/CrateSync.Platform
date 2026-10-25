using FluentValidation;

namespace CrateSync.Platform.TradingPartners.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.CommercialIdentifier).MaximumLength(100);

        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.StateOrProvince).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(30);
        RuleFor(x => x.Country).MaximumLength(100);

        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
    }
}
