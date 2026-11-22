using FluentValidation;

namespace CrateSync.Platform.Catalog.Application.Products.ActivateVariety;

internal sealed class ActivateProductVarietyCommandValidator
    : AbstractValidator<ActivateProductVarietyCommand>
{
    public ActivateProductVarietyCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VarietyId)
            .NotEmpty();
    }
}
