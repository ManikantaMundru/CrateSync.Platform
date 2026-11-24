using FluentValidation;

namespace CrateSync.Platform.InboundLogistics.Application.Shipments.CreateWarehouseDelivery;

internal sealed class CreateWarehouseDeliveryCommandValidator
    : AbstractValidator<CreateWarehouseDeliveryCommand>
{
    public CreateWarehouseDeliveryCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.TransportCompany).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VehicleNumber).MaximumLength(50);
        RuleFor(x => x.OriginLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DestinationLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TransportReference).MaximumLength(100);
        RuleFor(x => x.Remarks).MaximumLength(500);
        RuleFor(x => x.Allocations).NotEmpty();

        RuleForEach(x => x.Allocations)
            .ChildRules(line =>
            {
                line.RuleFor(x => x.ShipmentLineId).NotEmpty();
                line.RuleFor(x => x.Quantity).GreaterThan(0);
            });
    }
}
