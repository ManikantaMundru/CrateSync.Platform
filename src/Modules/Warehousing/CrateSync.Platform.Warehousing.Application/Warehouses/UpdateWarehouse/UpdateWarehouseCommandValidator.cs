using FluentValidation;

namespace CrateSync.Platform.Warehousing.Application.Warehouses.UpdateWarehouse;

internal sealed class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();

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
