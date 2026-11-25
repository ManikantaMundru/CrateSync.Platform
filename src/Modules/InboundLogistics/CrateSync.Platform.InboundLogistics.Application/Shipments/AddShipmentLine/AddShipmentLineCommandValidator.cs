using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.AddShipmentLine;

internal sealed class AddShipmentLineCommandValidator : AbstractValidator<AddShipmentLineCommand>
{
    public AddShipmentLineCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.ProductVarietyId)
            .NotEmpty();

        RuleFor(x => x.BatchCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ExpectedQuantity)
            .GreaterThan(0);

        RuleFor(x => x).Must(x => x.PackSizeKg.HasValue != x.FruitsPerPack.HasValue)
            .WithMessage("Specify either kilograms per pack or fruit count per pack.");
        RuleFor(x => x.PackSizeKg).GreaterThan(0m).When(x => x.PackSizeKg.HasValue);
        RuleFor(x => x.FruitsPerPack).GreaterThan(0).When(x => x.FruitsPerPack.HasValue);

        RuleFor(x => x).Must(x => x.UnitCost.HasValue == (x.Currency is not null))
            .WithMessage("Purchase cost and currency must be supplied together.");
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0m).LessThanOrEqualTo(9999999999999999.99m)
            .When(x => x.UnitCost.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Za-z]{3}$")
            .When(x => x.Currency is not null);
    }
}
