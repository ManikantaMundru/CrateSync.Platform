using FluentValidation;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.CreateWarehouse;

internal sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);

        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StateOrProvince).MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
    }
}
