using FluentValidation;

namespace CrateSync.Platform.Catalog.Application.Products.DeactivateVariety;

internal sealed class DeactivateProductVarietyCommandValidator
    : AbstractValidator<DeactivateProductVarietyCommand>
{
    public DeactivateProductVarietyCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VarietyId)
            .NotEmpty();
    }
}
