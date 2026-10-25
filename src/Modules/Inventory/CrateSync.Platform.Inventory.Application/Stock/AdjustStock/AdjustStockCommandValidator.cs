using FluentValidation;

namespace CrateSync.Platform.Inventory.Application.Stock.AdjustStock;

internal sealed class AdjustStockCommandValidator :
    AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ProductVarietyId).NotEmpty();

        RuleFor(x => x.QuantityChange)
            .NotEqual(0);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(250);
    }
}
