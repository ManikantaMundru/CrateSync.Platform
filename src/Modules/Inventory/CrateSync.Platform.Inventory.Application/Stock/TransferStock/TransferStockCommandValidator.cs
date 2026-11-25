using FluentValidation;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

internal sealed class TransferStockCommandValidator
    : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty();
        RuleFor(x => x.TransportCompany).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DriverPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VehicleNumber).MaximumLength(50);
        RuleFor(x => x.SourceWarehouseId).NotEmpty();
        RuleFor(x => x.DestinationWarehouseId).NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                x.SourceWarehouseId !=
                x.DestinationWarehouseId)
            .WithMessage(
                "Source and destination warehouses must be different.");

        RuleFor(x => x.ProductVarietyId).NotEmpty();
        RuleFor(x => x.OriginCountry)
            .NotEmpty()
            .Length(2);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.TransferReference)
            .MaximumLength(100);
        RuleFor(x => x.Remarks)
            .MaximumLength(250);
    }
}
