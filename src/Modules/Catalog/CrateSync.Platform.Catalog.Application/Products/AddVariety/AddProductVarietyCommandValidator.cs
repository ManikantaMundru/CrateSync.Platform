using FluentValidation;

namespace CrateSync.Platform.Catalog.Application.Products.AddVariety;

internal sealed class AddProductVarietyCommandValidator
    : AbstractValidator<AddProductVarietyCommand>
{
    public AddProductVarietyCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}
