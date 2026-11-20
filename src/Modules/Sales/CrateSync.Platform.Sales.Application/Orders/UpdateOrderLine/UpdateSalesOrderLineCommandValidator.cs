using FluentValidation;

namespace CrateSync.Platform.Sales.Application.Orders.UpdateOrderLine;

internal sealed class UpdateSalesOrderLineCommandValidator : AbstractValidator<UpdateSalesOrderLineCommand>
{
    public UpdateSalesOrderLineCommandValidator()
    {
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.SalesOrderLineId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PackSize).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
