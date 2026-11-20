using CrateSync.Platform.BuildingBlocks.Application;
using CrateSync.Platform.BuildingBlocks.Application.Commands;
using CrateSync.Platform.BuildingBlocks.Application.Common;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Application.Orders.Common;
using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.Orders.CancelOrder;

internal sealed class CancelSalesOrderCommandHandler(
    ISalesOrderRepository repository,
    ISalesUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
    : ICommandHandler<CancelSalesOrderCommand, Result>
{
    public async Task<Result> Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(
            SalesOrderId.From(request.SalesOrderId),
            tenantContext.TenantId,
            cancellationToken);

        if (order is null)
            return Result.Failure(SalesErrors.OrderNotFound);

        try
        {
            order.Cancel(timeProvider.GetUtcNow());
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(SalesErrors.Conflict(ex.Message));
        }
    }
}
