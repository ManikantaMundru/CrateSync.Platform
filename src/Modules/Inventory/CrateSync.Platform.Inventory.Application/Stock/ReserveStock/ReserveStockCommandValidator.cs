using FluentValidation;

namespace CrateSync.Platform.Inventory.Application.Stock.ReserveStock;

internal sealed class ReserveStockCommandValidator :
    AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ProductVarietyId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
