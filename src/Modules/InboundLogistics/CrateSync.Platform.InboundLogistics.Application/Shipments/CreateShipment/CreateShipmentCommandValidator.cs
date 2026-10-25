using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateShipment;

internal sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty();

        RuleFor(x => x.WarehouseId)
            .NotEmpty();

        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.OriginCountry)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.ExpectedArrivalAtUtc)
            .NotEmpty();
    }
}
