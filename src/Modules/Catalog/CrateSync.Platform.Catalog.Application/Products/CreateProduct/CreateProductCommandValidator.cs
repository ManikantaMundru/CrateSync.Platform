using FluentValidation;

namespace CrateSync.Platform.Catalog.Application.Products.CreateProduct;

internal sealed class CreateProductCommandValidator: AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}
