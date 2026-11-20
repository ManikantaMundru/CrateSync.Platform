using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Application.Orders.Common;
using CrateSync.Platform.Sales.Domain.Orders;
using CrateSync.Platform.Sales.Domain.ValueObjects;

namespace CrateSync.Platform.Sales.Application.Orders.UpdateOrderLine;

internal sealed class UpdateSalesOrderLineCommandHandler(
    ISalesOrderRepository repository,
    ISalesUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateSalesOrderLineCommand, Result>
{
    public async Task<Result> Handle(UpdateSalesOrderLineCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(
            SalesOrderId.From(request.SalesOrderId),
            tenantContext.TenantId,
            cancellationToken);

        if (order is null)
            return Result.Failure(SalesErrors.OrderNotFound);

        try
        {
            order.UpdateLine(
                SalesOrderLineId.From(request.SalesOrderLineId),
                new CrateQuantity(request.Quantity),
                new PackSize(request.PackSize),
                new Money(request.UnitPrice, new Currency(request.Currency)),
                timeProvider.GetUtcNow());

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(SalesErrors.Conflict(ex.Message));
        }
    }
}
