using FluentValidation;

namespace CrateSync.Platform.Inventory.Application.Stock.TransferStock;

internal sealed class TransferStockCommandValidator
    : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty();
        RuleFor(x => x.SourceWarehouseId).NotEmpty();
        RuleFor(x => x.DestinationWarehouseId).NotEmpty();

        RuleFor(x => x)
            .Must(x =>
                x.SourceWarehouseId !=
                x.DestinationWarehouseId)
            .WithMessage(
                "Source and destination warehouses must be different.");

        RuleFor(x => x.ProductVarietyId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.TransferReference)
            .MaximumLength(100);
        RuleFor(x => x.Remarks)
            .MaximumLength(250);
    }
}
