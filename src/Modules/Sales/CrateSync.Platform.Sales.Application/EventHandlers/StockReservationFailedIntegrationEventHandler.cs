using CrateSync.Platform.BuildingBlocks.Application.Events;
using CrateSync.Platform.IntegrationContracts.Inventory;
using CrateSync.Platform.Sales.Application.Abstractions;
using CrateSync.Platform.Sales.Domain.Orders;

namespace CrateSync.Platform.Sales.Application.EventHandlers;

internal sealed class StockReservationFailedIntegrationEventHandler(
    ISalesOrderRepository repository,
    ISalesUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IIntegrationEventHandler<StockReservationFailedIntegrationEvent>
{
    public async Task Handle(
        StockReservationFailedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(
            SalesOrderId.From(notification.SalesOrderId),
            notification.TenantId,
            cancellationToken);

        if (order is null)
            return;

        order.RecordReservationFailure(
            notification.Reason,
            timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
