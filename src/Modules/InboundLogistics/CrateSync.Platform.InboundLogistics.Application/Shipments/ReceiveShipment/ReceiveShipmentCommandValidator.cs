using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.ReceiveShipment;

internal sealed class ReceiveShipmentCommandValidator : AbstractValidator<ReceiveShipmentCommand>
{
    public ReceiveShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty();

        RuleFor(x => x.ShipmentLineId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
