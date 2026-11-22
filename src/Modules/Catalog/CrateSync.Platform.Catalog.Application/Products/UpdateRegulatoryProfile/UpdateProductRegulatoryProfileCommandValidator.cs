using FluentValidation;

namespace CrateSync.Platform.Catalog.Application.Products.UpdateRegulatoryProfile;

internal sealed class UpdateProductRegulatoryProfileCommandValidator
    : AbstractValidator<UpdateProductRegulatoryProfileCommand>
{
    public UpdateProductRegulatoryProfileCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.CustomsTariffCode)
            .MaximumLength(20);

        RuleFor(x => x.ScientificName)
            .MaximumLength(200);

        RuleFor(x => x.RegulatoryCategory)
            .MaximumLength(100);

        RuleFor(x => x.DefaultUnitOfMeasure)
            .MaximumLength(50);
    }
}
