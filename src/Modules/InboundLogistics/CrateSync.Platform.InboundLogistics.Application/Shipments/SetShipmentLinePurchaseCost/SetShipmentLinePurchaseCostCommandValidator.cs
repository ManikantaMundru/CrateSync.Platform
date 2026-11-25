using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.SetShipmentLinePurchaseCost;

internal sealed class SetShipmentLinePurchaseCostCommandValidator : AbstractValidator<SetShipmentLinePurchaseCostCommand>
{
    public SetShipmentLinePurchaseCostCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.UnitCost).InclusiveBetween(0m, 9999999999999999.99m).PrecisionScale(18, 2, true);
        RuleFor(x => x.Currency).NotEmpty().Matches("^[A-Za-z]{3}$");
    }
}
