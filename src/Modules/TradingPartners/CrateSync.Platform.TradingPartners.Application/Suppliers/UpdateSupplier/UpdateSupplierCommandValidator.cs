using FluentValidation;

namespace CrateSync.Platform.TradingPartners.Application.Suppliers.UpdateSupplier;

internal sealed class UpdateSupplierCommandValidator
    : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CommercialIdentifier)
            .MaximumLength(100);

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200);

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200);

        RuleFor(x => x.City)
            .MaximumLength(100);

        RuleFor(x => x.StateOrProvince)
            .MaximumLength(100);

        RuleFor(x => x.PostalCode)
            .MaximumLength(30);

        RuleFor(x => x.Country)
            .MaximumLength(100);

        RuleFor(x => x.ContactName)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        When(x => !string.IsNullOrWhiteSpace(x.AddressLine1), () =>
        {
            RuleFor(x => x.City).NotEmpty();
            RuleFor(x => x.PostalCode).NotEmpty();
            RuleFor(x => x.Country).NotEmpty();
        });

        When(x =>
            !string.IsNullOrWhiteSpace(x.City) ||
            !string.IsNullOrWhiteSpace(x.PostalCode) ||
            !string.IsNullOrWhiteSpace(x.Country), () =>
            {
                RuleFor(x => x.AddressLine1).NotEmpty();
            });
    }
}
