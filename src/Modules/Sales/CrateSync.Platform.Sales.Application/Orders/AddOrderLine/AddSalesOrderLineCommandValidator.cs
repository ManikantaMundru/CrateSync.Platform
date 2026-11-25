using FluentValidation;

namespace CrateSync.Platform.Sales.Application.Orders.AddOrderLine;

internal sealed class AddSalesOrderLineCommandValidator : AbstractValidator<AddSalesOrderLineCommand>
{
    public AddSalesOrderLineCommandValidator()
    {
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.ProductVarietyId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PackSize).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.OriginCountry).NotEmpty().Length(2);
    }
}
