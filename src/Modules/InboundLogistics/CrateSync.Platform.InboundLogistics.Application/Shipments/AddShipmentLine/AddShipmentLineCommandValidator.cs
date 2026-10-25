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

        RuleFor(x => x.PackSizeKg)
            .GreaterThan(0);

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);
    }
}
